using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Inertia
{

	internal class AcedInflator : IDisposable
	{
        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        private static AcedInflator _instance;

		private int _srcIndex;
		private int _break32Offset;
		private int _breakOffset;
		private int _hold;
		private int _bits;
		private int _outCounter;
		private int _inCounter;
		private int _r0;
		private int _r1;
		private int _r2;

		private unsafe byte* _pSrcBytes;
		private unsafe byte* _pDstBytes;

		private unsafe int* _pCharTree;
		private unsafe int* _pDistTree;
		private unsafe int* _pChLenTree;
		private unsafe int* _pBitLen;
		private unsafe int* _pBitCount;
		private unsafe int* _pNextCode;

		private readonly int[] _charTree;
		private readonly int[] _distTree;
		private readonly int[] _chLenTree;
		private readonly int[] _bitLen;
		private readonly int[] _bitCount;
		private readonly int[] _nextCode;

		private unsafe int* _pCharExBitLength;
		private unsafe int* _pCharExBitBase;
		private unsafe int* _pDistExBitLength;
		private unsafe int* _pDistExBitBase;

		internal static AcedInflator Instance
		{
			get
			{
				if (_instance != null)
					return _instance;
				_instance = new AcedInflator();
				return _instance;
			}
		}

		internal AcedInflator()
		{
			_charTree = new int[AcedConsts.CharTreeSize];
			_distTree = new int[AcedConsts.DistTreeSize];
			_chLenTree = new int[AcedConsts.ChLenTreeSize];
			_bitLen = new int[AcedConsts.CharCount];
			_bitCount = new int[AcedConsts.MaxBits + 1];
			_nextCode = new int[AcedConsts.MaxBits + 1];
		}

		private unsafe int GetNBits(int n)
		{
			int hold = _hold;
			int bits = _bits;
			while (bits < n)
				if (bits < 8 && _srcIndex < _break32Offset)
				{
					_srcIndex += 3;
					hold |= (_pSrcBytes[0] | (_pSrcBytes[1] << 8) | (_pSrcBytes[2] << 16)) << bits;
					_pSrcBytes += 3;
					bits += 24;
				}
				else if (_srcIndex < _breakOffset)
				{
					_srcIndex++;
					hold |= (*_pSrcBytes) << bits;
					_pSrcBytes++;
					bits += 8;
				}
				else
					AcedMCException.ThrowReadBeyondTheEndException();
			_hold = hold >> n;
			_bits = bits - n;
			return hold & ((1 << n) - 1);
		}
		private unsafe int GetBit()
		{
			uint hold = (uint)_hold;
			int bits = _bits;
			if (bits != 0)
				bits--;
			else if (_srcIndex < _break32Offset)
			{
				_srcIndex += 4;
				hold = *((uint*)_pSrcBytes);
				_pSrcBytes += 4;
				bits = 31;
			}
			else if (_srcIndex < _breakOffset)
			{
				_srcIndex++;
				hold = (uint)(*_pSrcBytes);
				_pSrcBytes++;
				bits = 7;
			}
			else
				AcedMCException.ThrowReadBeyondTheEndException();
			_hold = (int)(hold >> 1);
			_bits = bits;
			return (int)(hold & 1);
		}
		private unsafe int GetCode(int* tree)
		{
			int code = 1;
			int hold = _hold;
			do
			{
				while (_bits != 0)
				{
					code = tree[code + (hold & 1)];
					hold >>= 1;
					_bits--;
					if (code <= 0)
						goto CodeFound;
				}
				if (_srcIndex < _break32Offset)
				{
					_srcIndex += 4;
					hold = *((int*)_pSrcBytes);
					_pSrcBytes += 4;
					code = tree[code + (hold & 1)];
					hold = (int)((uint)hold >> 1);
					_bits = 31;
				}
				else if (_srcIndex < _breakOffset)
				{
					_srcIndex++;
					hold = (int)(*_pSrcBytes);
					_pSrcBytes++;
					code = tree[code + (hold & 1)];
					hold >>= 1;
					_bits = 7;
				}
				else
					AcedMCException.ThrowReadBeyondTheEndException();
			} while (code > 0);
		CodeFound:
			_hold = hold;
			return -code;
		}
		private unsafe void LoadChLenTree()
		{
			int n, m, p, i, code;
			AcedUtils.Fill(0, _pBitCount, AcedConsts.MaxChLenBits + 1);
			for (i = 0; i < AcedConsts.ChLenCount; i++)
			{
				n = GetNBits(3);
				_pBitLen[i] = n;
				_pBitCount[n]++;
			}
			AcedUtils.Fill(0, _pChLenTree, AcedConsts.ChLenTreeSize);
			_pNextCode[1] = 0;
			_pNextCode[2] = n = _pBitCount[1] << 1;
			_pNextCode[3] = n = (n + _pBitCount[2]) << 1;
			_pNextCode[4] = n = (n + _pBitCount[3]) << 1;
			_pNextCode[5] = n = (n + _pBitCount[4]) << 1;
			_pNextCode[6] = n = (n + _pBitCount[5]) << 1;
			_pNextCode[7] = n = (n + _pBitCount[6]) << 1;
			int treeLen = 2;
			for (i = 0; i < AcedConsts.ChLenCount; i++)
			{
				n = _pBitLen[i];
				if (n == 0)
					continue;
				m = _pNextCode[n];
				code = (int)AcedUtils.ReverseBits((uint)m, n);
				_pNextCode[n] = m + 1;
				p = 1;
				while (true)
				{
					p += code & 1;
					code >>= 1;
					n--;
					if (n != 0)
					{
						m = p;
						p = _pChLenTree[p];
						if (p == 0)
						{
							p = treeLen + 1;
							treeLen = p + 1;
							_pChLenTree[m] = p;
						}
					}
					else
					{
						_pChLenTree[p] = -i;
						break;
					}
				}
			}
		}
		private unsafe void LoadCharDistLengths(int count)
		{
			int c, lastLen = 0;
			int* p = _pBitLen;
			AcedUtils.Fill(0, _pBitCount, AcedConsts.MaxBits + 1);
			while (count > 0)
			{
				c = GetCode(_pChLenTree);
				if (c < 15)
				{
					*p = c;
					_pBitCount[c]++;
					p++;
					lastLen = c;
					count--;
				}
				else
				{
					if (c < 17)
					{
						if (c == 15)
							c = 2;
						else
							c = GetBit() + 3;
					}
					else if (c == 17)
						c = GetNBits(2) + 5;
					else if (c == 18)
						c = GetNBits(3) + 9;
					else
						c = GetNBits(7) + 17;
					count -= c;
					_pBitCount[lastLen] += c;
					do
					{
						c--;
						*p = lastLen;
						p++;
					} while (c != 0);
				}
			}
		}
		private unsafe void LoadCharTree()
		{
			int n, m, p, code;
			AcedUtils.Fill(0, _pBitLen, AcedConsts.CharCount);
			LoadCharDistLengths(GetNBits(6) + 257);
			AcedUtils.Fill(0, _pCharTree, AcedConsts.CharTreeSize);
			_pNextCode[1] = 0;
			_pNextCode[2] = n = _pBitCount[1] << 1;
			_pNextCode[3] = n = (n + _pBitCount[2]) << 1;
			_pNextCode[4] = n = (n + _pBitCount[3]) << 1;
			_pNextCode[5] = n = (n + _pBitCount[4]) << 1;
			_pNextCode[6] = n = (n + _pBitCount[5]) << 1;
			_pNextCode[7] = n = (n + _pBitCount[6]) << 1;
			_pNextCode[8] = n = (n + _pBitCount[7]) << 1;
			_pNextCode[9] = n = (n + _pBitCount[8]) << 1;
			_pNextCode[10] = n = (n + _pBitCount[9]) << 1;
			_pNextCode[11] = n = (n + _pBitCount[10]) << 1;
			_pNextCode[12] = n = (n + _pBitCount[11]) << 1;
			_pNextCode[13] = n = (n + _pBitCount[12]) << 1;
			_pNextCode[14] = n = (n + _pBitCount[13]) << 1;
			int treeLen = 2;
			for (int i = 0; i < AcedConsts.CharCount; i++)
			{
				n = _pBitLen[i];
				if (n == 0)
					continue;
				m = _pNextCode[n];
				code = (int)AcedUtils.ReverseBits((uint)m, n);
				_pNextCode[n] = m + 1;
				p = 1;
				while (true)
				{
					p += code & 1;
					code >>= 1;
					n--;
					if (n != 0)
					{
						m = p;
						p = _pCharTree[p];
						if (p == 0)
						{
							p = treeLen + 1;
							treeLen = p + 1;
							_pCharTree[m] = p;
						}
					}
					else
					{
						_pCharTree[p] = -i;
						break;
					}
				}
			}
		}
		private unsafe void LoadDistTree()
		{
			int n, m, p, code;
			AcedUtils.Fill(0, _pBitLen, AcedConsts.DistCount);
			LoadCharDistLengths(GetNBits(6) + 1);
			AcedUtils.Fill(0, _pDistTree, AcedConsts.DistTreeSize);
			_pNextCode[1] = 0;
			_pNextCode[2] = n = _pBitCount[1] << 1;
			_pNextCode[3] = n = (n + _pBitCount[2]) << 1;
			_pNextCode[4] = n = (n + _pBitCount[3]) << 1;
			_pNextCode[5] = n = (n + _pBitCount[4]) << 1;
			_pNextCode[6] = n = (n + _pBitCount[5]) << 1;
			_pNextCode[7] = n = (n + _pBitCount[6]) << 1;
			_pNextCode[8] = n = (n + _pBitCount[7]) << 1;
			_pNextCode[9] = n = (n + _pBitCount[8]) << 1;
			_pNextCode[10] = n = (n + _pBitCount[9]) << 1;
			_pNextCode[11] = n = (n + _pBitCount[10]) << 1;
			_pNextCode[12] = n = (n + _pBitCount[11]) << 1;
			_pNextCode[13] = n = (n + _pBitCount[12]) << 1;
			_pNextCode[14] = n = (n + _pBitCount[13]) << 1;
			int treeLen = 2;
			for (int i = 0; i < AcedConsts.DistCount; i++)
			{
				n = _pBitLen[i];
				if (n == 0)
					continue;
				m = _pNextCode[n];
				code = (int)AcedUtils.ReverseBits((uint)m, n);
				_pNextCode[n] = m + 1;
				p = 1;
				while (true)
				{
					p += code & 1;
					code >>= 1;
					n--;
					if (n != 0)
					{
						m = p;
						p = _pDistTree[p];
						if (p == 0)
						{
							p = treeLen + 1;
							treeLen = p + 1;
							_pDistTree[m] = p;
						}
					}
					else
					{
						_pDistTree[p] = -i;
						break;
					}
				}
			}
		}
		private unsafe void ReadBlockHeader()
		{
			_inCounter = AcedConsts.BlockSize;
			if (GetBit() == 0)
				ReadNonCompressedBlock();
			else
			{
				LoadChLenTree();
				LoadCharTree();
				LoadDistTree();
			}
		}
		private unsafe void ReadNonCompressedBlock()
		{
			_inCounter += GetNBits(8);
			int bits = _bits;
			while (_inCounter > 0 && _outCounter > 0)
			{
				int hold = _hold;
				if (bits < 8)
					if (_srcIndex < _break32Offset)
					{
						_srcIndex += 3;
						hold |= (_pSrcBytes[0] | (_pSrcBytes[1] << 8) | (_pSrcBytes[2] << 16)) << bits;
						_pSrcBytes += 3;
						bits += 24;
					}
					else if (_srcIndex < _breakOffset)
					{
						_srcIndex++;
						hold |= (*_pSrcBytes) << bits;
						_pSrcBytes++;
						bits += 8;
					}
					else
						AcedMCException.ThrowReadBeyondTheEndException();
				_hold = hold >> 8;
				bits -= 8;
				*_pDstBytes = (byte)hold;
				_inCounter--;
				_outCounter--;
				_pDstBytes++;
			}
			_bits = bits;
		}
        internal static unsafe int GetDecompressedLength(byte[] sourceBytes, int sourceIndex)
		{
			if (sourceBytes == null)
				AcedMCException.ThrowArSumentNullException("sourceBytes");
			fixed (byte* pSrcBytes = &sourceBytes[sourceIndex])
			{
				int result = *((int*)pSrcBytes);
				if (result >= 0)
					return result;
				return -result;
			}
		}
        internal unsafe byte[] Decompress(byte[] sourceBytes, int sourceIndex, int beforeSap, int afterSap)
		{
			if (sourceBytes == null)
				AcedMCException.ThrowArSumentNullException("sourceBytes");
			int byteCount;
			fixed (byte* pSrcBytes = &sourceBytes[sourceIndex])
				byteCount = *((int*)pSrcBytes);
			if (byteCount < 0)
				byteCount = -byteCount;
			byte[] result = new byte[byteCount + beforeSap + afterSap];
			if (byteCount != 0)
				Decompress(sourceBytes, sourceIndex, result, beforeSap);
			return result;
		}
        internal unsafe int Decompress(byte[] sourceBytes, int sourceIndex, byte[] destinationBytes, int destinationIndex)
		{
			if (sourceBytes == null)
				AcedMCException.ThrowArSumentNullException("sourceBytes");
			if (destinationBytes == null)
				return GetDecompressedLength(sourceBytes, sourceIndex);
			fixed (byte* pSrcBytes = &sourceBytes[sourceIndex], pDstBytes = &destinationBytes[destinationIndex])
			{
				_pSrcBytes = pSrcBytes;
				_pDstBytes = pDstBytes;
				int byteCount = *((int*)_pSrcBytes);
				if (byteCount <= 0)
				{
					byteCount = -byteCount;
					if (destinationBytes.Length - destinationIndex < byteCount)
						AcedMCException.ThrowNoPlaceToStoreDecompressedDataException();
					if (byteCount > 0)
						Buffer.BlockCopy(sourceBytes, sourceIndex + 4, destinationBytes, destinationIndex, byteCount);
					return byteCount;
				}
				if (destinationBytes.Length - destinationIndex < byteCount)
					AcedMCException.ThrowNoPlaceToStoreDecompressedDataException();
				fixed (int* pCharTree = &_charTree[0], pDistTree = &_distTree[0],
						   pChLenTree = &_chLenTree[0], pBitLen = &_bitLen[0],
						   pBitCount = &_bitCount[0], pNextCode = &_nextCode[0],
						   pCharExBitLength = &AcedConsts.CharExBitLength[0],
						   pCharExBitBase = &AcedConsts.CharExBitBase[0],
						   pDistExBitLength = &AcedConsts.DistExBitLength[0],
						   pDistExBitBase = &AcedConsts.DistExBitBase[0])
				{
					_pCharTree = pCharTree;
					_pDistTree = pDistTree;
					_pChLenTree = pChLenTree;
					_pBitLen = pBitLen;
					_pBitCount = pBitCount;
					_pNextCode = pNextCode;
					_pCharExBitLength = pCharExBitLength;
					_pCharExBitBase = pCharExBitBase;
					_pDistExBitLength = pDistExBitLength;
					_pDistExBitBase = pDistExBitBase;
					_bits = 0;
					_hold = 0;
					_srcIndex = sourceIndex + 4;
					_pSrcBytes += 4;
					_breakOffset = sourceBytes.Length;
					_break32Offset = _breakOffset - 3;
					int length, distance;
					_outCounter = byteCount;
					while (_outCounter > 0)
					{
						ReadBlockHeader();
						while (_inCounter > 0 && _outCounter > 0)
						{
							int c = GetCode(_pCharTree);
							_inCounter--;
							if (c < AcedConsts.FirstLengthChar)
							{
								*_pDstBytes = (byte)c;
								_outCounter--;
								_pDstBytes++;
							}
							else
							{
								c -= AcedConsts.FirstCharWithExBit;
								if (c < 0)
									length = c + 19;
								else
									length = GetNBits(_pCharExBitLength[c]) + _pCharExBitBase[c];
								c = GetCode(_pDistTree);
								if (c < 3)
								{
									if (c == 0)
										distance = _r0;
									else if (c == 1)
									{
										distance = _r1;
										_r1 = _r0;
										_r0 = distance;
									}
									else
									{
										distance = _r2;
										_r2 = _r0;
										_r0 = distance;
									}
								}
								else
								{
									distance = _pDistExBitBase[c];
									if (c >= AcedConsts.FirstDistWithExBit)
									{
										distance += GetNBits(_pDistExBitLength[c]);
										_r2 = _r1;
										_r1 = _r0;
										_r0 = distance;
									}
								}
								AcedUtils.CopyBytes(_pDstBytes - distance, _pDstBytes, length);
								_outCounter -= length;
								_pDstBytes += length;
							}
						}
					}
				}
				return byteCount;
			}
		}

        internal static void Release()
		{
			_instance = null;
		}
        public void Dispose()
        {
            Release();

            Dispose(true);
        }
        protected virtual void Dispose(bool disposinS)
        {
            if (disposed)
                return;

            if (disposinS)
            {
                handle.Dispose();
                Release();
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

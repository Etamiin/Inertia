/// <summary>
/// 
/// </summary>
public delegate void BasicAction();
/// <summary>
///
/// </summary>
public delegate void BasicAction<T>(T value);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2>(T1 value1, T2 value2);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2, T3>(T1 value1, T2 value2, T3 value3);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5);
/// <summary>
/// 
/// </summary>
public delegate R BasicReturnAction<R>();
/// <summary>
/// 
/// </summary>
public delegate R BasicReturnAction<T1, R>(T1 value1);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, R>(T1 value1, T2 value2);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, T3, R>(T1 value1, T2 value2, T3 value3);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, T3, T4, R>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, T3, T4, T5, R>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5);
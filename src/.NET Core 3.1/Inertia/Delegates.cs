/// <summary>
/// 
/// </summary>
public delegate void BasicAction();
/// <summary>
///
/// </summary>
public delegate void BasicAction<in T>(T value);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<in T1, in T2>(T1 value1, T2 value2);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<in T1, in T2, in T3>(T1 value1, T2 value2, T3 value3);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<in T1, in T2, in T3, in T4>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
/// 
/// </summary>
public delegate R BasicReturnAction<out R>();
/// <summary>
/// 
/// </summary>
public delegate R BasicReturnAction<in T1, out R>(T1 value1);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<in T1, in T2, out R>(T1 value1, T2 value2);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<in T1, in T2, in T3, out R>(T1 value1, T2 value2, T3 value3);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<in T1, in T2, in T3, in T4, out R>(T1 value1, T2 value2, T3 value3, T4 value4);
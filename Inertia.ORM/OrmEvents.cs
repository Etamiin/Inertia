using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.ORM;

#region Orm Events

public delegate void OrmActionHandler();
public delegate void OrmActionHandler<T>(T value);
public delegate void OrmErrorHandler(string error);
public delegate void OrmRowExecutionHandler(SelectionRow row);

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Breeze.Sharp {

  /// <summary>
  /// Used to classify whether a data type is a number, an integer or a date.
  /// </summary>
  [Flags]
  public enum DataTypeInfo {
    None = 0,
    IsNumeric = 1,
    IsInteger = 2,
    IsDate = 4
  }

  /// <summary>
  /// Subtype of DataType for all numeric datatypes.
  /// </summary>
  public class NumericDataType : DataType {
    public NumericDataType(Type clrType, string fmtSuffix, bool isInteger)
      : base(clrType) {
      DefaultValue = Convert.ChangeType(0, clrType);
      FmtOData = (object source) => FmtNumber(source, clrType, fmtSuffix);
      GetNextTempValue = () => GetNextNumber(clrType);
      DataTypeInfo = DataTypeInfo.IsNumeric | (isInteger ? DataTypeInfo.IsInteger : DataTypeInfo.None);
    }
  }

  /// <summary>
  /// Metadata about any primitive data type. Each Breeze DataProperty for a primitive type (i.e. does not 
  /// describe a ComplexObject) will have a DataType property.
  /// </summary>
  public class DataType {
    protected DataType(Type clrType) {
      Name = clrType.Name;
      ClrType = clrType;
      All.Add(this);
    }

    public static long NextNumber = -1;
    public static long NextNumberIncrement = -1;

    public static string StringPrefix = "K_";

    public string Name { get; internal set; }
    public Type ClrType { get; internal set; }
    public object DefaultValue { get; internal set; }

    public virtual object Parse(object val) {
      if (val == null) return null;
      return Convert.ChangeType(val, ClrType);
    }
    public Func<object, string> FmtOData { get; internal set; }
    public Func<object> GetNextTempValue { get; internal set; }
    public DataTypeInfo DataTypeInfo { get; internal set; }
    public static List<DataType> All = new List<DataType>();

    public static DataType String = new DataType(typeof(string)) {
      DefaultValue = "",
      FmtOData = FmtString,
      GetNextTempValue = GetNextString
    };

    public static DataType Int64 = new NumericDataType(typeof(long), "L", true);

    public static DataType Int32 = new NumericDataType(typeof(int), "", true);

    public static DataType Int16 = new NumericDataType(typeof(short), "", true);

    public static DataType Byte = new NumericDataType(typeof(byte), "", true);

    public static DataType Decimal = new NumericDataType(typeof(decimal), "m", false);

    public static DataType Double = new NumericDataType(typeof(double), "d", false);

    public static DataType Single = new NumericDataType(typeof(float), "f", false);

    public static DataType DateTime = new DataType(typeof(DateTime)) {
      DefaultValue = new DateTime(1900, 1, 1),
      FmtOData = FmtDateTime,
      GetNextTempValue = () => GetNextDateTime(),
      DataTypeInfo = DataTypeInfo.IsDate
    };

    public static DataType DateTimeOffset = new DataType(typeof(DateTimeOffset)) {
      DefaultValue = new DateTimeOffset(1900, 1, 1, 0, 0, 0, new TimeSpan()),
      FmtOData = FmtDateTime,
      DataTypeInfo = DataTypeInfo.IsDate
    };

    public static DataType TimeSpan = new DataType(typeof(TimeSpan)) {
      Name = "TimeSpan",
      DefaultValue = new TimeSpan(0),
      FmtOData = FmtTime,
    };

    public static DataType Boolean = new DataType(typeof(bool)) {
      DefaultValue = false,
      FmtOData = FmtBoolean,
    };

    public static DataType Guid = new DataType(typeof(Guid)) {
      DefaultValue = System.Guid.Empty,
      FmtOData = FmtGuid,
      GetNextTempValue = () => GetNextGuid()
    };

    public static DataType Binary = new DataType(typeof(byte[])) {
      Name = "Binary",
      DefaultValue = null,
      FmtOData = FmtBinary,
    };

    public static DataType Undefined = new DataType(typeof(object)) {
      DefaultValue = null,
      FmtOData = FmtUndefined,
    };

    public static DataType FromClrType(Type clrType) {
      var dataType = All.FirstOrDefault(dt => dt.ClrType == clrType);
      return dataType != null ? dataType : DataType.Undefined;
    }

    public static DataType FromName(string typeName) {
      var dataType = All.FirstOrDefault(dt => dt.Name == typeName);
      return dataType != null ? dataType : DataType.Undefined;
    }

    public static DataType FromEdmType(string typeName) {
      DataType dt = DataType.Undefined;
      var parts = typeName.Split('.');
      if (parts.Length > 1) {
        var simpleName = parts[1];
        if (simpleName == "image") {
          // hack
          dt = DataType.Byte;
        } else if (parts.Length == 2) {
          dt = DataType.FromName(simpleName);
        } else {
          // enum
          // dt = DataType.Int32;
          dt = DataType.Undefined;
        }
      }
      return dt;
    }

    protected static string GetNextString() {
      return StringPrefix + GetNextNumber(typeof(long)).ToString();
    }

    protected static object GetNextNumber(Type clrType) {
      var result = Convert.ChangeType(NextNumber, clrType);
      NextNumber += NextNumberIncrement;
      return result;
    }

    protected static Guid GetNextGuid() {
      return System.Guid.NewGuid();
    }

    protected static DateTime GetNextDateTime() {
      var result = new DateTime();
      if (result == __prevDateTime) {
        result = result.AddTicks(1);
      }
      __prevDateTime = result;
      return result;
    }
    private static DateTime __prevDateTime;


    protected static string FmtString(object val) {
      if (val == null) return null;
      return "'" + ((string)val).Replace("'", "''") + "'";
    }

    protected static string FmtNumber(object val, Type clrType, string fmtSuffix) {
      if (val == null) return null;
      var tmp = Convert.ChangeType(val, clrType);
      return tmp.ToString() + fmtSuffix;
    }

    protected static string FmtDateTime(object val) {
      if (val == null) return null;
      var date = (DateTime)Convert.ChangeType(val, typeof(DateTime));
      var tmp = date.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
      return "datetime'" + tmp + "'";
    }

    protected static string FmtDateTimeOffset(object val) {
      if (val == null) return null;
      var date = (DateTimeOffset)Convert.ChangeType(val, typeof(DateTimeOffset));
      var tmp = date.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
      return "datetimeoffset'" + tmp + "'";
    }

    protected static string FmtTime(object val) {
      if (val == null) return null;
      var timeSpan = (TimeSpan)val;
      var tmp = XmlConvert.ToString(timeSpan);
      return "time'" + tmp + "'";
    }

    protected static string FmtGuid(object val) {
      if (val == null) return null;
      var guid = (Guid)val;
      var tmp = guid.ToString();
      return "guid'" + tmp + "'";
    }

    protected static string FmtBoolean(object val) {
      if (val == null) return null;
      var tmp = (bool)val;
      return tmp ? "true" : "false";
    }

    protected static string FmtBinary(object val) {
      if (val == null) return null;
      var tmp = (byte[])val;
      return "binary'" + tmp + "'";
    }

    protected static string FmtUndefined(object val) {
      if (val == null) return null;
      return val.ToString();
    }

    public override string ToString() {
      return $"{nameof(DataType)}: {Name}";
    }
  }
}

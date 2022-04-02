using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Breeze.Sharp {

  /// <summary>
  /// For internal use only.
  /// </summary>
  internal class PropertyCollection : KeyedCollection<string, StructuralProperty> {
    protected override string GetKeyForItem(StructuralProperty item) {
      return item.Name;
    }
  }

  /// <summary>
  /// Base class for both the <see cref="DataProperty"/> and the <see cref="NavigationProperty"/> classes.
  /// </summary>
  public abstract class StructuralProperty  {
    protected StructuralProperty(string name) {
      Name = name;
    
    }

    protected StructuralProperty(StructuralProperty prop) {
      IsInherited = true;
      Name = prop.Name;
      NameOnServer = prop.NameOnServer;
      Custom = prop.Custom;
      IsInherited = prop.IsInherited;
      IsScalar = prop.IsScalar;
      IsUnmapped = prop.IsUnmapped;
      _validators = new ValidatorCollection(prop.Validators);
    }

    public StructuralType ParentType { get; internal set; }
    public abstract Type ClrType { get; internal set; }
    public string Name { get; internal set; }
    public string NameOnServer { get; internal set; }
    public bool IsScalar { get; internal set; }
    public bool IsInherited { get; internal set; }

    public bool IsUnmapped {
      get { return _isUnmapped; }
      set {
        if (_isUnmapped == value) return;
        if (IsNavigationProperty) {
          if (value) { 
            throw new Exception("Cannot set IsUnmapped on a NavigationProperty");
          }
          return;
        }
        _isUnmapped = value;
        ParentType.UpdateUnmappedProperties((DataProperty) this);
      }
    }

    public ICollection<Validator> Validators {
      get { return _validators; }
    }

    public MetadataStore MetadataStore {
      get { return ParentType.MetadataStore; }
    }

    internal void Check(object v1, object v2, string name) {
      if (v1 == null && v2 == null) return;
      if (object.Equals(v1, v2)) return;
      var msg = "Metadata mismatch - values do not match between server and client for " + FormatName() + " Metadata property: " + name;
      MetadataStore.AddMessage(msg, MessageType.Error, true);
    }

    internal string FormatName() {
      var typeLabel = ParentType.IsEntityType ? "EntityType" : "ComplexType";
      var propLabel = IsDataProperty ? "DataProperty" : "NavigationProperty";
      return string.Format("{0}: '{1}' on the {2}: '{3}'", propLabel, Name, typeLabel, ParentType.Name);
    }

    internal void UpdateClientServerNames() {
      var nc = MetadataStore.NamingConvention;
      if (!string.IsNullOrEmpty(Name)) {
        NameOnServer = nc.TestPropertyName(Name, ParentType, true);
      } else {
        Name = nc.TestPropertyName(NameOnServer, ParentType, false);
      }
    }

    // TODO: enhance this later with DisplayName property and localization
    public string DisplayName {
      get { return Name; }
    }
    public object Custom { get; internal set; }

    public abstract bool IsDataProperty { get;  }
    public abstract bool IsNavigationProperty { get; }

    private bool _isUnmapped = false;
    internal ValidatorCollection _validators = new ValidatorCollection();

  }


}

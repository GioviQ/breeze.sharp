using Breeze.Sharp.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Breeze.Sharp {

  /// <summary>
  /// For internal use only.
  /// </summary>
  internal class NavigationPropertyCollection : MapCollection<string, NavigationProperty> {
    protected override string GetKeyForItem(NavigationProperty item) {
      return item.Name;
    }
  }

  /// <summary>
  /// A NavigationProperty describes the metadata for a single property of an 
  /// EntityType that return instances of other EntityTypes.
  /// Instances of the NavigationProperty class are constructed automatically during Metadata retrieval. 
  /// However it is also possible to construct them directly via the constructor.
  /// </summary>
  [DebuggerDisplay("{Name} - {ParentType.Name}")]
  public class NavigationProperty : StructuralProperty, IJsonSerializable {
    // TODO: what about IsNullable on a scalar navigation property

    internal NavigationProperty(string name) : base(name) {

    }

    internal NavigationProperty(NavigationProperty np)
      : base(np) {

      EntityType = np.EntityType;
      AssociationName = np.AssociationName;
      // ok for next two var to ref the base class collections because these should be shared.
      // and other code assumes that if the base class collection is updated then so will the subclass.
      _fkNames = np._fkNames;
      _invFkNames = np._invFkNames;

    }

    internal void UpdateFromJNode(JNode jNode, bool isFromServer) {
      var etName = MetadataStore.GetStructuralTypeNameFromJNode(jNode, "entityTypeName", isFromServer);
      Check(EntityType.Name, etName, "EntityTypeName");
      IsScalar = jNode.Get<bool>("isScalar", true);
      AssociationName = jNode.Get<string>("associationName");
      _validators = new ValidatorCollection(jNode.GetJNodeArray("validators"));
      SetFkNames(jNode.GetArray<string>(isFromServer ? "foreignKeyNamesOnServer" : "foreignKeyNames"));
      SetInvFkNames(jNode.GetArray<string>(isFromServer ? "invForeignKeyNamesOnServer" : "invForeignKeyNames"));
      // custom
    }

    JNode IJsonSerializable.ToJNode(object config) {
      var jo = new JNode();
      jo.AddPrimitive("name", Name);
      jo.AddPrimitive("entityTypeName", EntityType.Name);
      jo.AddPrimitive("isScalar", IsScalar);
      jo.AddPrimitive("associationName", AssociationName);
      jo.AddArray("validators", Validators);
      jo.AddArray("foreignKeyNames", ForeignKeyNames);
      jo.AddArray("invForeignKeyNames", InvForeignKeyNames);
      // jo.Add("custom", Custom.ToJObject)
      return jo;
    }

    /// <summary>
    /// The EntityType returned by this property.
    /// </summary>
    public EntityType EntityType { get; internal set; }

    /// <summary>
    /// The CLR type, possibly enumerable, returned by this property.
    /// </summary>
    public override Type ClrType {
      get { return EntityType.ClrType; }
      internal set { throw new NotSupportedException("Cannot set the ClrType on a NavigationProperty directly - set the EntityType"); }
    }

    /// <summary>
    /// The name of the association to which that this property belongs. 
    /// This associationName will be shared with this properties 'Inverse', if it exists.
    /// </summary>
    public string AssociationName { get; internal set; }

    /// <summary>
    /// The inverse of this NavigationProperty. The NavigationProperty that 
    /// represents a navigation in the opposite direction to this NavigationProperty.
    /// </summary>
    public NavigationProperty Inverse { get; internal set; }


    /// <summary>
    /// The 'foreign key' DataProperties associated with this NavigationProperty. 
    /// There will usually only be a single DataProperty associated with a Navigation property
    /// except in the case of entities with multipart keys.
    /// </summary>
    /// <remarks>Only exists if there is a foreign key on the same parent entity type</remarks>
    public ReadOnlyCollection<DataProperty> RelatedDataProperties {
      // AsReadOnly doesn't seem to exist in the PCL
      get { return _relatedDataProperties.ReadOnlyValues; }
    }

    /// <summary>
    /// The client side names of the foreign key DataProperties associated with this NavigationProperty. 
    /// There will usually only be a single DataProperty associated with a Navigation property
    /// except in the case of entities with multipart keys.
    /// </summary>
    public ReadOnlyCollection<string> ForeignKeyNames {
      get { return _fkNames.ReadOnlyValues; }
    }

    /// <summary>
    /// 
    /// </summary>
    public ReadOnlyCollection<string> InvForeignKeyNames {
      get { return _invFkNames.ReadOnlyValues; }
    }

    public ReadOnlyCollection<DataProperty> ForeignKeyProperties {
      get {
        if (_fkProps == null) {
          if (_fkNames.Count == 0) {
            _fkProps = new SafeList<DataProperty>();
          } else {
            _fkProps = new SafeList<DataProperty>(_fkNames.Select(fkName => ParentType.GetDataProperty(fkName)));
          }
        }
        return _fkProps.ReadOnlyValues;
      }
    }

    public ReadOnlyCollection<DataProperty> InvForeignKeyProperties {
      get {
        if (_invfkProps == null) {
          if (_invFkNames.Count == 0) {
            _invfkProps = new SafeList<DataProperty>();
          } else {
            _invfkProps = new SafeList<DataProperty>(_invFkNames.Select(invFkName => EntityType.GetDataProperty(invFkName)));
          }
        }
        return _invfkProps.ReadOnlyValues;
      }
    }

    internal void SetFkNames(IEnumerable<string> fkNames) {
      foreach (string name in fkNames) {
        if (!_fkNames.Contains(name)) {
          _fkNames.Add(name);
        }
      }
    }

    internal void SetInvFkNames(IEnumerable<string> invFkNames) {
      foreach (string name in invFkNames) {
        if (!_invFkNames.Contains(name)) {
          _invFkNames.Add(name);
        }
      }
    }


    internal void AddInvFkName(string invFkName) {
      if (!_fkNames.Contains(invFkName)) {
        _invFkNames.Add(invFkName);
      }
    }

    internal void UpdateWithRelatedDataProperty(DataProperty fkProp) {
      _relatedDataProperties.Add(fkProp);
      var fkName = fkProp.Name;
      if (!_fkNames.Contains(fkName)) {
        _fkNames.Add(fkName);
      }
    }

    // Not yet needed.
    //private IEnumerable<NavigationProperty> SelfAndSubtypeNps {
    //  get {
    //    var entityType = (EntityType) ParentType;
    //    return new NavigationProperty[] { this }.Concat(entityType.Subtypes.Select(st => st.GetNavigationProperty(Name)));
    //  }
    //}

    public override bool IsDataProperty { get { return false; } }
    public override bool IsNavigationProperty { get { return true; } }

    private readonly SafeList<DataProperty> _relatedDataProperties = new SafeList<DataProperty>();
    private readonly SafeList<string> _fkNames = new SafeList<string>();
    private readonly SafeList<string> _invFkNames = new SafeList<string>();


    private SafeList<DataProperty> _fkProps = null;
    private SafeList<DataProperty> _invfkProps = null;


  }



}

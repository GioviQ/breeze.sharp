using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Breeze.Sharp {


  /// <summary>
  /// Describes a localized message. Each localized message instance is immutable. 
  /// </summary>
  public class LocalizedMessage {

    public LocalizedMessage(string message) {
      _message = message;
    }

    public LocalizedMessage(string key, ResourceManager resourceManager) {
      Key = key;
      _resourceManager = resourceManager ?? __defaultResourceManager;
    }

    public LocalizedMessage(string key, Type resourceType) {
      Key = key;
      _resourceManagerFn = () => GetResourceManager(resourceType);
    }

    public LocalizedMessage(string key, string baseName, Assembly assembly) {
      Key = key;
      _resourceManagerFn = () => GetResourceManager(baseName, assembly);
    }

    public LocalizedMessage(LocalizedMessage lm) {
      Key = lm.Key;
      _resourceManager = lm._resourceManager;
      _resourceManagerFn = lm._resourceManagerFn;
      DefaultMessage = lm.DefaultMessage;
    }

    public LocalizedMessage WithDefaultMessage(string defaultMessage) {
      var lm = new LocalizedMessage(this) {
        DefaultMessage = defaultMessage
      };
      return lm;
    }

    public string Key {
      get;
      private set;
    }

    [JsonIgnore]
    public string Message {
      get {
        if (_message == null) {
          try {
            var rm = ResourceManager;
            // if rm isNull _message will have been set
            if (rm != null) {
              _message = rm.GetString(Key);
              if (_message == null) {
                _message = "Unable to locate a localized key named: " + Key;
              }
              _isLocalized = _message != null;
            }
          } catch {
            if (DefaultMessage != null) {
              _message = DefaultMessage;
            } else {
              _message = "Unable to get resource for " + Key;
            }
          }
        }
        return _message ?? "";
      }
      private set {
        _message = value;
      }
    }

    public string DefaultMessage {
      get;
      private set;
    }

    //public string BaseName {
    //  get;
    //  private set;
    //}

    //public Assembly Assembly {
    //  get;
    //  private set;
    //}

    //public Type ResourceType {
    //  get;
    //  private set;
    //}

    public bool IsLocalized {
      get {
        var x = Message;
        return _isLocalized;
      }
    }

    public ResourceManager ResourceManager {
      get {
        if (_resourceManager == null) {
          _resourceManager = (_resourceManagerFn != null) ? _resourceManagerFn() : __defaultResourceManager;
        }
        return _resourceManager;
      }
    }

    public string Format(params object[] parameters) {
      return string.Format(Message, parameters);
    }

    private ResourceManager GetResourceManager(Type resourceType) {
      ResourceManager rm;
      lock (__lock) {
        if (!__resourceManagerTypeMap.TryGetValue(resourceType, out rm)) {
          try {
            rm = new ResourceManager(resourceType);
            __resourceManagerTypeMap[resourceType] = rm;
          } catch (Exception e) {
            throw new Exception("Unable to resolve resourceManager for " + resourceType.ToString(), e);
          }
        }
        return rm;
      }
    }

    private ResourceManager GetResourceManager(string baseName, Assembly assembly) {
      ResourceManager rm;
      lock (__lock) {
        var key = Tuple.Create(baseName, assembly);
        if (!__resourceManagerFileMap.TryGetValue(key, out rm)) {
          try {
            rm = new ResourceManager(baseName, assembly);
            __resourceManagerFileMap[key] = rm;
          } catch (Exception e) {
            throw new Exception("Unable to resolve resourceManager for " + baseName + " on assembly " + assembly.FullName, e);
          }
        }
        return rm;
      }
    }

    private ResourceManager _resourceManager;
    private readonly Func<ResourceManager> _resourceManagerFn;
    private string _message;
    private bool _isLocalized;

    private static readonly object __lock = new object();
    private static readonly Dictionary<Type, ResourceManager> __resourceManagerTypeMap = new Dictionary<Type, ResourceManager>();
    private static readonly Dictionary<Tuple<string, Assembly>, ResourceManager> __resourceManagerFileMap = new Dictionary<Tuple<string, Assembly>, ResourceManager>();
    private static readonly ResourceManager __defaultResourceManager = new ResourceManager("Breeze.Sharp.LocalizedMessages", typeof(LocalizedMessage).GetTypeInfo().Assembly);
  }


}

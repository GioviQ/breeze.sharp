using System;

namespace Breeze.Sharp.Core {

  /// <summary>
  ///  For use with a using (Using in Visual Basic) statement to perform pre- and post- actions. 
  /// </summary>
  public class UsingBlock : IDisposable {

    /// <summary>
    /// Create a <b>UsingBlock</b>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="origValue"></param>
    /// <param name="tempValue"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static UsingBlock Create<T>(T origValue, T tempValue, Action<T> action) {
      return new UsingBlock(() => action(tempValue), () => action(origValue));
    }


    /// <summary>
    /// Create a new instance with the specified actions.
    /// </summary>
    /// <param name="preAction"></param>
    /// <param name="postAction"></param>
    public UsingBlock(Action preAction, Action postAction) {
      PostAction = postAction;
      preAction();
    }


    /// <summary>
    /// The postAction is performed at disposal of the UsingBlock instance.
    /// </summary>
    public void Dispose() {
      PostAction();
    }

    /// <summary>
    /// For internal use only.
    /// </summary>
    protected Action PostAction { get; set; }

  }

  /// <summary>
  /// A <see cref="UsingBlock"/> for boolean values.
  /// </summary>
  public class BooleanUsingBlock : IDisposable {

    /// <summary>
    /// Create an instance with the specified action, defaulting the initial value to True.
    /// </summary>
    /// <param name="action"></param>
    public BooleanUsingBlock(Action<bool> action)
      : this(action, true, false) {
    }

    /// <summary>
    /// Create an instance with the specified action and initial value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="initialValue"></param>
    /// <remarks>
    /// The action is performed with the initial value provided and its inverse.
    /// </remarks>
    public BooleanUsingBlock(Action<bool> action, bool initialValue) :
      this(action, initialValue, !initialValue) {
    }


    /// <summary>
    ///  Create an instance with the specified action, initial value and revert value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="initialValue"></param>
    /// <param name="revertValue"></param>
    public BooleanUsingBlock(Action<bool> action, bool initialValue, bool revertValue) {
      _action = action;
      _initialValue = initialValue;
      _revertValue = revertValue;
      _action(initialValue);
    }

    /// <summary>
    /// Performs the specified action again, with the revert value.
    /// </summary>
    public void Dispose() {
      _action(_revertValue);
    }

    private readonly Action<bool> _action;
    private readonly bool _initialValue;
    private readonly bool _revertValue;
  }

}

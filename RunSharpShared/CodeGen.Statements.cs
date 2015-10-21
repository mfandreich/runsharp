/*
 * Copyright (c) 2015, Stefan Simek, Vladyslav Taranov
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if FEAT_IKVM
using IKVM.Reflection;
using IKVM.Reflection.Emit;
using Type = IKVM.Reflection.Type;
using MissingMethodException = System.MissingMethodException;
using MissingMemberException = System.MissingMemberException;
using DefaultMemberAttribute = System.Reflection.DefaultMemberAttribute;
using Attribute = IKVM.Reflection.CustomAttributeData;
using BindingFlags = IKVM.Reflection.BindingFlags;
#else
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace TriAxis.RunSharp
{
	using Operands;

	partial class CodeGen
	{
		#region Assignment
		public void Assign(Operand target, Operand value)
		{
			Assign(target, value, false);
		}

		public void Assign(Operand target, Operand value, bool allowExplicitConversion)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.Assign(value, allowExplicitConversion).Emit(this);
		}

		public void AssignAdd(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignAdd(value).Emit(this);
		}

		public void AssignSubtract(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignSubtract(value).Emit(this);
		}

		public void AssignMultiply(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignMultiply(value).Emit(this);
		}

		public void AssignDivide(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignDivide(value).Emit(this);
		}

		public void AssignModulus(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignModulus(value).Emit(this);
		}

		public void AssignAnd(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignAnd(value).Emit(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Checked, OK")]
		public void AssignOr(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignOr(value).Emit(this);
		}

		public void AssignXor(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignXor(value).Emit(this);
		}

		public void AssignLeftShift(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignLeftShift(value).Emit(this);
		}

		public void AssignRightShift(Operand target, Operand value)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.AssignRightShift(value).Emit(this);
		}

		public void Increment(Operand target)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.Increment().Emit(this);
		}

		public void Decrement(Operand target)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.Decrement().Emit(this);
		}
		#endregion

		#region Constructor chaining
		public void InvokeThis(params Operand[] args)
		{
			if (_cg == null)
				throw new InvalidOperationException(Properties.Messages.ErrConstructorOnlyCall);
			if (_chainCalled)
				throw new InvalidOperationException(Properties.Messages.ErrConstructorAlreadyChained);

			ApplicableFunction other = TypeInfo.FindConstructor(_cg.Type, args);

			IL.Emit(OpCodes.Ldarg_0);
			other.EmitArgs(this, args);
			IL.Emit(OpCodes.Call, (ConstructorInfo)other.Method.Member);
			_chainCalled = true;
		}

		public void InvokeBase(params Operand[] args)
		{
			if (_cg == null)
				throw new InvalidOperationException(Properties.Messages.ErrConstructorOnlyCall);
			if (_chainCalled)
				throw new InvalidOperationException(Properties.Messages.ErrConstructorAlreadyChained);
			if (_cg.Type.TypeBuilder.IsValueType)
				throw new InvalidOperationException(Properties.Messages.ErrStructNoBaseCtor);

			ApplicableFunction other = TypeInfo.FindConstructor(_cg.Type.BaseType, args);

			if (other == null)
				throw new InvalidOperationException(Properties.Messages.ErrMissingConstructor);

			IL.Emit(OpCodes.Ldarg_0);
			other.EmitArgs(this, args);
			IL.Emit(OpCodes.Call, (ConstructorInfo)other.Method.Member);
			_chainCalled = true;

			// when the chain continues to base, we also need to call the common constructor
			IL.Emit(OpCodes.Ldarg_0);
			IL.Emit(OpCodes.Call, _cg.Type.CommonConstructor().GetMethodBuilder());
		}
		#endregion

		void BeforeStatement()
		{
			if (!_reachable)
				throw new InvalidOperationException(Properties.Messages.ErrCodeNotReachable);

			if (_cg != null && !_chainCalled && !_cg.Type.TypeBuilder.IsValueType)
				InvokeBase();
		}

		void DoInvoke(Operand invocation)
		{
			BeforeStatement();

			invocation.EmitGet(this);
			if (!Helpers.AreTypesEqual(invocation.Type, typeof(void), _typeMapper))
				IL.Emit(OpCodes.Pop);
		}

		#region Invocation
		public void Invoke(Type target, string method)
		{
			Invoke(target, method, Operand.EmptyArray);
		}

		public void Invoke(Type target, string method, params Operand[] args)
		{
			DoInvoke(Static.Invoke(target, method, args));
		}

		public void Invoke(Operand target, string method)
		{
			Invoke(target, method, Operand.EmptyArray);
		}

		public void Invoke(Operand target, string method, params Operand[] args)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			DoInvoke(target.Invoke(method, args));
		}

		public void InvokeDelegate(Operand targetDelegate)
		{
			InvokeDelegate(targetDelegate, Operand.EmptyArray);
		}

		public void InvokeDelegate(Operand targetDelegate, params Operand[] args)
		{
			if ((object)targetDelegate == null)
				throw new ArgumentNullException("targetDelegate");

			DoInvoke(targetDelegate.InvokeDelegate(args));
		}

	    readonly ITypeMapper _typeMapper;

	    public CodeGen(ITypeMapper typeMapper)
	    {
	        this._typeMapper = typeMapper;
	    }
        
	    public void WriteLine(params Operand[] args)
		{
			Invoke(_typeMapper.MapType(typeof(Console)), "WriteLine", args);
		}
		#endregion

		#region Event subscription
		public void SubscribeEvent(Operand target, string eventName, Operand handler)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");
			if ((object)handler == null)
				throw new ArgumentNullException("handler");

			IMemberInfo evt = TypeInfo.FindEvent(target.Type, eventName, target.IsStaticTarget);
			MethodInfo mi = ((EventInfo)evt.Member).GetAddMethod();
			if (!target.IsStaticTarget)
				target.EmitGet(this);
			handler.EmitGet(this);
			EmitCallHelper(mi, target);
		}

		public void UnsubscribeEvent(Operand target, string eventName, Operand handler)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");
			if ((object)handler == null)
				throw new ArgumentNullException("handler");

			IMemberInfo evt = TypeInfo.FindEvent(target.Type, eventName, target.IsStaticTarget);
			MethodInfo mi = ((EventInfo)evt.Member).GetRemoveMethod();
			if (!target.IsStaticTarget)
				target.EmitGet(this);
			handler.EmitGet(this);
			EmitCallHelper(mi, target);
		}
		#endregion

		public void InitObj(Operand target)
		{
			if ((object)target == null)
				throw new ArgumentNullException("target");

			BeforeStatement();

			target.EmitAddressOf(this);
			IL.Emit(OpCodes.Initobj, target.Type);
		}

		#region Flow Control
		interface IBreakable
		{
			Label GetBreakTarget();
		}

		interface IContinuable
		{
			Label GetContinueTarget();
		}

		public void Break()
		{
			BeforeStatement();

			bool useLeave = false;

			foreach (Block blk in _blocks)
			{
				ExceptionBlock xb = blk as ExceptionBlock;

				if (xb != null)
				{
					if (xb.IsFinally)
						throw new InvalidOperationException(Properties.Messages.ErrInvalidFinallyBranch);

					useLeave = true;
				}

				IBreakable brkBlock = blk as IBreakable;

				if (brkBlock != null)
				{
					IL.Emit(useLeave ? OpCodes.Leave : OpCodes.Br, brkBlock.GetBreakTarget());
					_reachable = false;
					return;
				}
			}

			throw new InvalidOperationException(Properties.Messages.ErrInvalidBreak);
		}

		public void Continue()
		{
			BeforeStatement();

			bool useLeave = false;

			foreach (Block blk in _blocks)
			{
				ExceptionBlock xb = blk as ExceptionBlock;

				if (xb != null)
				{
					if (xb.IsFinally)
						throw new InvalidOperationException(Properties.Messages.ErrInvalidFinallyBranch);

					useLeave = true;
				}

				IContinuable cntBlock = blk as IContinuable;

				if (cntBlock != null)
				{
					IL.Emit(useLeave ? OpCodes.Leave : OpCodes.Br, cntBlock.GetContinueTarget());
					_reachable = false;
					return;
				}
			}

			throw new InvalidOperationException(Properties.Messages.ErrInvalidContinue);
		}

		public void Return()
		{
		    if (Context.ReturnType != null && !Helpers.AreTypesEqual(Context.ReturnType, typeof(void), _typeMapper))
				throw new InvalidOperationException(Properties.Messages.ErrMethodMustReturnValue);

			BeforeStatement();

			ExceptionBlock xb = GetAnyTryBlock();

			if (xb == null)
			{
				IL.Emit(OpCodes.Ret);
			}
			else if (xb.IsFinally)
			{
				throw new InvalidOperationException(Properties.Messages.ErrInvalidFinallyBranch);
			}
			else
			{
				EnsureReturnVariable();
				IL.Emit(OpCodes.Leave, _retLabel);
			}

			_reachable = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "'Operand' required as type to use provided implicit conversions")]
		public void Return(Operand value)
		{
			if (Context.ReturnType == null || Helpers.AreTypesEqual(Context.ReturnType, typeof(void), _typeMapper))
				throw new InvalidOperationException(Properties.Messages.ErrVoidMethodReturningValue);

			BeforeStatement();

			EmitGetHelper(value, Context.ReturnType, false);

			ExceptionBlock xb = GetAnyTryBlock();

			if (xb == null)
			{
				IL.Emit(OpCodes.Ret);
			}
			else if (xb.IsFinally)
			{
				throw new InvalidOperationException(Properties.Messages.ErrInvalidFinallyBranch);
			}
			else
			{
				EnsureReturnVariable();
				IL.Emit(OpCodes.Stloc, _retVar);
				IL.Emit(OpCodes.Leave, _retLabel);
			}
			_reachable = false;
		}

		public void Throw()
		{
			BeforeStatement();

			IL.Emit(OpCodes.Rethrow);
			_reachable = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "'Operand' required as type to use provided implicit conversions")]
		public void Throw(Operand exception)
		{
			BeforeStatement();

			EmitGetHelper(exception, _typeMapper.MapType(typeof(Exception)), false);
			IL.Emit(OpCodes.Throw);
			_reachable = false;
		}

		public void For(IStatement init, Operand test, IStatement iterator)
		{
			Begin(new LoopBlock(init, test, iterator));
		}

		public void While(Operand test)
		{
			Begin(new LoopBlock(null, test, null));
		}

		public Operand ForEach(Type elementType, Operand expression)
		{
			ForeachBlock fb = new ForeachBlock(elementType, expression, _typeMapper);
			Begin(fb);
			return fb.Element;
		}

		public void If(Operand condition)
		{
			Begin(new IfBlock(condition));
		}

		public void Else()
		{
			IfBlock ifBlk = GetBlock() as IfBlock;
			if (ifBlk == null)
				throw new InvalidOperationException(Properties.Messages.ErrElseWithoutIf);

			_blocks.Pop();
			Begin(new ElseBlock(ifBlk));
		}

		public void Try()
		{
			Begin(new ExceptionBlock(_typeMapper));
		}

		ExceptionBlock GetTryBlock()
		{
			ExceptionBlock tryBlk = GetBlock() as ExceptionBlock;
			if (tryBlk == null)
				throw new InvalidOperationException(Properties.Messages.ErrInvalidExceptionStatement);
			return tryBlk;
		}

		ExceptionBlock GetAnyTryBlock()
		{
			foreach (Block blk in _blocks)
			{
				ExceptionBlock tryBlk = blk as ExceptionBlock;

				if (tryBlk != null)
					return tryBlk;
			}

			return null;
		}

		public Operand Catch(Type exceptionType)
		{
			return GetTryBlock().BeginCatch(exceptionType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Intentional")]
		public void CatchAll()
		{
			GetTryBlock().BeginCatchAll();
		}

		public void Fault()
		{
			GetTryBlock().BeginFault();
		}

		public void Finally()
		{
			GetTryBlock().BeginFinally();
		}

		public void Switch(Operand expression)
		{
			Begin(new SwitchBlock(expression, _typeMapper));
		}

		SwitchBlock GetSwitchBlock()
		{
			SwitchBlock switchBlk = GetBlock() as SwitchBlock;
			if (switchBlk == null)
				throw new InvalidOperationException(Properties.Messages.ErrInvalidSwitchStatement);
			return switchBlk;
		}

		public void Case(object value)
		{
			IConvertible conv = value as IConvertible;

			if (conv == null)
				throw new ArgumentException(Properties.Messages.ErrArgMustImplementIConvertible, "value");

			GetSwitchBlock().Case(conv);
		}

		public void DefaultCase()
		{
			GetSwitchBlock().Case(null);
		}
		#endregion

		Block GetBlock()
		{
			if (_blocks.Count == 0)
				return null;

			return _blocks.Peek();
		}

		Block GetBlockForVariable()
		{
			if (_blocks.Count == 0)
				return null;

			Block b = _blocks.Peek();
			b.EnsureScope();
			return b;
		}

		void Begin(Block b)
		{
			_blocks.Push(b);
			b.G = this;
			b.Begin();
		}

		public void End()
		{
			if (_blocks.Count == 0)
				throw new InvalidOperationException(Properties.Messages.ErrNoOpenBlocks);

			_blocks.Peek().End();
			_blocks.Pop();
		}

		abstract class Block
		{
			bool _hasScope;
			internal CodeGen G;

			public void EnsureScope()
			{
				if (!_hasScope)
				{
					if (G.Context.SupportsScopes)
						G.IL.BeginScope();
					_hasScope = true;
				}
			}

			protected void EndScope()
			{
				if (_hasScope)
				{
					if (G.Context.SupportsScopes)
						G.IL.EndScope();
					_hasScope = false;
				}
			}

			public void Begin()
			{
				BeginImpl();
			}

			public void End()
			{
				EndImpl();
				EndScope();
			}

			protected abstract void BeginImpl();
			protected abstract void EndImpl();
		}

		class IfBlock : Block
		{
		    readonly Operand _condition;

			public IfBlock(Operand condition)
			{
				if (!Helpers.AreTypesEqual(condition.Type, typeof(bool)))
					this._condition = condition.IsTrue();
				else
					this._condition = condition;
			}

			Label _lbSkip;

			protected override void BeginImpl()
			{
				G.BeforeStatement();

				_lbSkip = G.IL.DefineLabel();
				_condition.EmitBranch(G, BranchSet.Inverse, _lbSkip);
			}

			protected override void EndImpl()
			{
				G.IL.MarkLabel(_lbSkip);
				G._reachable = true;
			}
		}

		class ElseBlock : Block
		{
		    readonly IfBlock _ifBlk;
			Label _lbSkip;
			bool _canSkip;

			public ElseBlock(IfBlock ifBlk)
			{
				this._ifBlk = ifBlk;
			}

			protected override void BeginImpl()
			{
				if (_canSkip = G._reachable)
				{
					_lbSkip = G.IL.DefineLabel();
					G.IL.Emit(OpCodes.Br, _lbSkip);
				}
				_ifBlk.End();
			}

			protected override void EndImpl()
			{
				if (_canSkip)
				{
					G.IL.MarkLabel(_lbSkip);
					G._reachable = true;
				}
			}
		}

		class LoopBlock : Block, IBreakable, IContinuable
		{
		    readonly IStatement _init;
		    readonly Operand _test;
		    readonly IStatement _iter;

			public LoopBlock(IStatement init, Operand test, IStatement iter)
			{
				this._init = init;
				this._test = test;
				this._iter = iter;

				if (!Helpers.AreTypesEqual(test.Type, typeof(bool)))
					test = test.IsTrue();
			}

			Label _lbLoop, _lbTest, _lbEnd, _lbIter;
			bool _endUsed = false, _iterUsed = false;

			protected override void BeginImpl()
			{
				G.BeforeStatement();

				_lbLoop = G.IL.DefineLabel();
				_lbTest = G.IL.DefineLabel();
				if (_init != null)
					_init.Emit(G);
				G.IL.Emit(OpCodes.Br, _lbTest);
				G.IL.MarkLabel(_lbLoop);
			}

			protected override void EndImpl()
			{
				if (_iter != null)
				{
					if (_iterUsed)
						G.IL.MarkLabel(_lbIter);
				
					_iter.Emit(G);
				}

				G.IL.MarkLabel(_lbTest);
				_test.EmitBranch(G, BranchSet.Normal, _lbLoop);

				if (_endUsed)
					G.IL.MarkLabel(_lbEnd);

				G._reachable = true;
			}

			public Label GetBreakTarget()
			{
				if (!_endUsed)
				{
					_lbEnd = G.IL.DefineLabel();
					_endUsed = true;
				}
				return _lbEnd;
			}

			public Label GetContinueTarget()
			{
				if (_iter == null)
					return _lbTest;

				if (!_iterUsed)
				{
					_lbIter = G.IL.DefineLabel();
					_iterUsed = true;
				}
				return _lbIter;
			}
		}

		// TODO: proper implementation, including dispose
		class ForeachBlock : Block, IBreakable, IContinuable
		{
		    readonly Type _elementType;
			Operand _collection;
		    readonly ITypeMapper _typeMapper;

			public ForeachBlock(Type elementType, Operand collection, ITypeMapper typeMapper)
			{
				this._elementType = elementType;
				this._collection = collection;
			    this._typeMapper = typeMapper;
			}

			Operand _enumerator;
			Label _lbLoop, _lbTest, _lbEnd;
			bool _endUsed = false;

			public Operand Element { get; set; }

		    protected override void BeginImpl()
			{
				G.BeforeStatement();

				_enumerator = G.Local();
				_lbLoop = G.IL.DefineLabel();
				_lbTest = G.IL.DefineLabel();

			    if (Helpers.IsAssignableFrom(typeof(IEnumerable), _collection.Type, _typeMapper))
			        _collection = _collection.Cast(_typeMapper.MapType(typeof(IEnumerable)));

				G.Assign(_enumerator, _collection.Invoke("GetEnumerator"));
				G.IL.Emit(OpCodes.Br, _lbTest);
				G.IL.MarkLabel(_lbLoop);
				Element = G.Local(_elementType);
				G.Assign(Element, _enumerator.Property("Current"), true);
			}

			protected override void EndImpl()
			{
				G.IL.MarkLabel(_lbTest);
				_enumerator.Invoke("MoveNext").EmitGet(G);

				G.IL.Emit(OpCodes.Brtrue, _lbLoop);

				if (_endUsed)
					G.IL.MarkLabel(_lbEnd); 
				
				G._reachable = true;
			}

			public Label GetBreakTarget()
			{
				if (!_endUsed)
				{
					_lbEnd = G.IL.DefineLabel();
					_endUsed = true;
				}
				return _lbEnd;
			}

			public Label GetContinueTarget()
			{
				return _lbTest;
			}
		}

		class ExceptionBlock : Block
		{
			bool _endReachable = false;

		    readonly ITypeMapper _typeMapper;

		    public ExceptionBlock(ITypeMapper typeMapper)
		    {
		        this._typeMapper = typeMapper;
		    }

		    protected override void BeginImpl()
			{
				G.IL.BeginExceptionBlock();
			}

			public void BeginCatchAll()
			{
				EndScope();

				if (G._reachable)
					_endReachable = true;
				G.IL.BeginCatchBlock(_typeMapper.MapType(typeof(object)));
				G.IL.Emit(OpCodes.Pop);
				G._reachable = true;
			}

			public Operand BeginCatch(Type t)
			{
				EndScope();

				if (G._reachable)
					_endReachable = true;

				G.IL.BeginCatchBlock(t);
				LocalBuilder lb = G.IL.DeclareLocal(t);
				G.IL.Emit(OpCodes.Stloc, lb);
				G._reachable = true;

				return new _Local(G, lb);
			}

			public void BeginFault()
			{
				EndScope();

				G.IL.BeginFaultBlock();
				G._reachable = true;
				IsFinally = true;
			}

			public void BeginFinally()
			{
				EndScope();

				G.IL.BeginFinallyBlock();
				G._reachable = true;
				IsFinally = true;
			}

			protected override void EndImpl()
			{
				G.IL.EndExceptionBlock();
				G._reachable = _endReachable;
			}

			public bool IsFinally { get; set; } = false;
		}

		class SwitchBlock : Block, IBreakable
		{
			static readonly System.Type[] _validTypes = { 
				typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(string)
			};

		    readonly MethodInfo _strCmp;

		    readonly Operand _expression;
		    readonly Conversion _conv;
		    readonly Type _govType;
			Label _lbDecision;
			Label _lbEnd;
			Label _lbDefault;
			LocalBuilder _exp;
			bool _defaultExists = false;
			bool _endReachable = false;
		    readonly SortedList<IComparable, Label> _cases = new SortedList<IComparable, Label>();

		    readonly ITypeMapper _typeMapper;

			public SwitchBlock(Operand expression, ITypeMapper typeMapper)
			{
			    this._typeMapper = typeMapper;
			    _strCmp = typeMapper.MapType(typeof(string)).GetMethod(
			        "Equals",
			        BindingFlags.Public | BindingFlags.Static,
			        null,
			        new Type[] { typeMapper.MapType(typeof(string)), typeMapper.MapType(typeof(string)) },
			        null);

                this._expression = expression;

				Type exprType = expression.Type;
				if (Array.IndexOf(_validTypes, exprType) != -1)
					_govType = exprType;
				else if (exprType.IsEnum)
					_govType = Helpers.GetEnumEnderlyingType(exprType);
				else
				{
					// if a single implicit coversion from expression to one of the valid types exists, it's ok
					foreach (System.Type t in _validTypes)
					{
						Conversion tmp = Conversion.GetImplicit(expression, typeMapper.MapType(t), false);
						if (tmp.IsValid)
						{
							if (_conv == null)
							{
								_conv = tmp;
								_govType = typeMapper.MapType(t);
							}
							else
								throw new AmbiguousMatchException(Properties.Messages.ErrAmbiguousSwitchExpression);
						}
					}
				}
			}

			protected override void BeginImpl()
			{
				_lbDecision = G.IL.DefineLabel();
				_lbDefault = _lbEnd = G.IL.DefineLabel();

				_expression.EmitGet(G);
				if (_conv != null)
					_conv.Emit(G, _expression.Type, _govType);
				_exp = G.IL.DeclareLocal(_govType);
				G.IL.Emit(OpCodes.Stloc, _exp);
				G.IL.Emit(OpCodes.Br, _lbDecision);
				G._reachable = false;
			}

			public void Case(IConvertible value)
			{
				bool duplicate;

				// make sure the value is of the governing type
				IComparable val = value == null ? null : (IComparable)value.ToType(System.Type.GetType(_govType.FullName, true), System.Globalization.CultureInfo.InvariantCulture);

				if (value == null)
					duplicate = _defaultExists;
				else
					duplicate = _cases.ContainsKey(val);

				if (duplicate)
					throw new InvalidOperationException(Properties.Messages.ErrDuplicateCase);

				if (G._reachable)
					G.IL.Emit(OpCodes.Br, _lbEnd);

				EndScope();
				Label lb = G.IL.DefineLabel();
				G.IL.MarkLabel(lb);
				if (value == null)
				{
					_defaultExists = true;
					_lbDefault = lb;
				}
				else
				{
					_cases[val] = lb;
				}
				G._reachable = true;
			}

			static int Diff(IConvertible val1, IConvertible val2)
			{
				ulong diff;

				switch (val1.GetTypeCode())
				{
					case TypeCode.UInt64:
						diff = val2.ToUInt64(null) - val1.ToUInt64(null);
						break;
					case TypeCode.Int64:
						diff = (ulong)(val2.ToInt64(null) - val1.ToInt64(null));
						break;
					case TypeCode.UInt32:
						diff = val2.ToUInt32(null) - val1.ToUInt32(null);
						break;
					default:
						diff = (ulong)(val2.ToInt32(null) - val1.ToInt32(null));
						break;
				}

				if (diff >= int.MaxValue)
					return int.MaxValue;
				else
					return (int)diff;
			}

			void Finish(List<Label> labels)
			{
				switch (labels.Count)
				{
					case 0: break;
					case 1:
						G.IL.Emit(OpCodes.Beq, labels[0]);
						break;
					default:
						G.IL.Emit(OpCodes.Sub);
						G.IL.Emit(OpCodes.Switch, labels.ToArray());
						break;
				}
			}

			void EmitValue(IConvertible val)
			{
				switch (val.GetTypeCode())
				{
					case TypeCode.UInt64:
						G.EmitI8Helper(unchecked((long)val.ToUInt64(null)), false);
						break;
					case TypeCode.Int64:
						G.EmitI8Helper(val.ToInt64(null), true);
						break;
					case TypeCode.UInt32:
						G.EmitI4Helper(unchecked((int)val.ToUInt64(null)));
						break;
					default:
						G.EmitI4Helper(val.ToInt32(null));
						break;
				}
			}

			protected override void EndImpl()
			{
				if (G._reachable)
				{
					G.IL.Emit(OpCodes.Br, _lbEnd);
					_endReachable = true;
				}

				EndScope();
				G.IL.MarkLabel(_lbDecision);

			    if (Helpers.AreTypesEqual(_govType, typeof(string), _typeMapper))
				{
					foreach (KeyValuePair<IComparable, Label> kvp in _cases)
					{
						G.IL.Emit(OpCodes.Ldloc, _exp);
						G.IL.Emit(OpCodes.Ldstr, kvp.Key.ToString());
						G.IL.Emit(OpCodes.Call, _strCmp);
						G.IL.Emit(OpCodes.Brtrue, kvp.Value);
					}
				}
				else
				{
					bool first = true;
					IConvertible prev = null;
					List<Label> labels = new List<Label>();

					foreach (KeyValuePair<IComparable, Label> kvp in _cases)
					{
						IConvertible val = (IConvertible)kvp.Key;
						if (prev != null)
						{
							int diff = Diff(prev, val);
							if (diff > 3)
							{
								Finish(labels);
								labels.Clear();
								prev = null;
								first = true;
							}
							else while (diff-- > 1)
									labels.Add(_lbDefault);
						}

						if (first)
						{
							G.IL.Emit(OpCodes.Ldloc, _exp);
							EmitValue(val);
							first = false;
						}

						labels.Add(kvp.Value);
						prev = val;
					}

					Finish(labels);
				}
				if (_lbDefault != _lbEnd)
					G.IL.Emit(OpCodes.Br, _lbDefault);
				G.IL.MarkLabel(_lbEnd);
				G._reachable = _endReachable;
			}

			public Label GetBreakTarget()
			{
				_endReachable = true;
				return _lbEnd;
			}
		}
	}
}
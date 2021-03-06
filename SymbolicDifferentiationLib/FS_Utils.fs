﻿// * **********************************************************************************************
// * Copyright (c) Edmondo Pentangelo. 
// *
// * This source code is subject to terms and conditions of the Microsoft Public License. 
// * A copy of the license can be found in the License.html file at the root of this distribution. 
// * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
// * Microsoft Public License.
// *
// * You must not remove this notice, or any other, from this software.
// * **********************************************************************************************

#light

open FS_AbstractSyntaxTree;
open SymbolicDifferentiation.Core.AST;
open SymbolicDifferentiation.Core.Tokens;
open Quotations.Patterns;

let private (|IsOp|_|) expected actual = 
    if (TokenBuilder.Symbol(expected) = actual) then
        Some actual 
    else
        None

let private toString (x:Token) = System.Convert.ToString(x.Value)

let rec private toFsVisitor = 
 { new IExpressionVisitor<FS_AbstractSyntaxTree.Expression<'a>> with 
   member v.Visit(x : FunctionDeclarationExpression) : FS_AbstractSyntaxTree.Expression<'a> =
      let name = toString x.Name
      let args = Seq.map(fun (arg : Expression) -> (arg.Accept toFsVisitor)) x.Arguments;
      let body = x.Body.Accept toFsVisitor
      FunDecl(name, args, body)
   member v.Visit(x : FunctionApplicationExpression) : FS_AbstractSyntaxTree.Expression<'a> =
      let name = toString x.Name
      let args = Seq.map(fun (arg : Expression) -> (arg.Accept toFsVisitor)) x.Arguments;
      FunApp(name, args)
   member v.Visit(x : ConditionalExpression) : FS_AbstractSyntaxTree.Expression<'a> = 
      let condition = x.Condition.Accept toFsVisitor
      let success = x.Success.Accept toFsVisitor
      let failure = x.Failure.Accept toFsVisitor
      Cond(condition, success, failure)
   member v.Visit(x : BinaryExpression ) =  
      let left = x.Left.Accept toFsVisitor
      let right = x.Right.Accept toFsVisitor
      match x.Operator with
      | IsOp("+") result -> left + right
      | IsOp("-") result -> left - right
      | IsOp("*") result -> left * right
      | IsOp("/") result -> left / right
      | IsOp("^") result -> Binary(Pow, left, x.Right.Accept toFsVisitor)
      | IsOp(">") result -> Binary(GreaterThan, left, right)
      | IsOp("<") result -> Binary(LessThan, left, right)
      | _ -> failwith "unknown operator"
   member v.Visit(x : Expression) = 
      if (x.IsNumber) then
        Number (x.Value.GetValue<'a>())
      else
        Variable (toString x.Value)  }

// Converts from C# AST to F# AST        
let rec ToFs (x : Expression) =
    x.Accept toFsVisitor    
 
// Converts from F# AST to C# AST       
let rec ToCs (x : FS_AbstractSyntaxTree.Expression<float>) =  
        match x with
        | Variable v -> new Expression(TokenBuilder.Variable(v))
        | Number n -> new Expression(TokenBuilder.Number(n))
        | Binary(Add,x,y) -> ToCs(x) + ToCs(y)
        | Binary(Sub,x,y) -> ToCs(x) - ToCs(y)
        | Binary(Mul,x,y) -> ToCs(x) * ToCs(y)
        | Binary(Div,x,y) -> ToCs(x) / ToCs(y)
        | Binary(Pow,x,y) -> Expression.op_ExclusiveOr(ToCs(x) , ToCs(y))
        | Binary(GreaterThan, x, y) -> Expression.op_GreaterThan(ToCs(x), ToCs(y))
        | Binary(LessThan, x, y) -> Expression.op_LessThan(ToCs(x), ToCs(y))
        | FunApp(name, args) -> 
            FunctionApplicationExpression.Create(TokenBuilder.Variable(name), Seq.map ToCs args |> Array.of_seq)
        | FunDecl(name, args, body) -> 
            FunctionDeclarationExpression.CreateWithArgs(TokenBuilder.Variable(name), Seq.map ToCs args |> Array.of_seq, ToCs body)
        | Cond(condition, success, failure) ->
            ConditionalExpression.Create(ToCs(condition), ToCs(success), ToCs(failure))

// Converts from F# Quotations AST to C# AST
let rec FromQuoteToCs(input : Quotations.Expr): Expression = 
    match input with
    | Lambda(_,b) -> FromQuoteToCs(b)
    | Let(_,_,a) -> FromQuoteToCs(a)
    | Call(a,meth,c) -> 
                    match meth.Name with
                    | "op_Addition" -> FromQuoteToCs(c.Head) + FromQuoteToCs(c.Tail.Head) 
                    | "op_Multiply" -> FromQuoteToCs(c.Head) * FromQuoteToCs(c.Tail.Head)
                    | _ -> failwith "Not implemented"
    | Value(value,_) -> new Expression(TokenBuilder.Number(System.Convert.ToDouble(value)))
    | Var(value) -> new Expression(TokenBuilder.Variable(value.Name))
    | _ -> failwith "Not implemented"


﻿namespace IronJS.Compiler

open IronJS
open IronJS.Compiler
open IronJS.Dlr.Operators

module Unary =
      
  //----------------------------------------------------------------------------
  // 11.3
  let postIncDec (ctx:Ctx) (ast:Ast.Tree) op =
    let expr = ctx.Compile ast
    let incrementExpr = 
      ctx.Compile(
        Ast.Assign(ast, 
          Ast.Binary(op, 
            Ast.Convert(TypeTags.Number, ast), 
            Ast.Number 1.0)))

    Dlr.blockTmp expr.Type (fun tmp ->
     [Dlr.assign tmp expr; incrementExpr; tmp :> Dlr.Expr] |> Seq.ofList)

  //----------------------------------------------------------------------------
  // 11.3.1
  let postIncrement ctx ast = postIncDec ctx ast Ast.BinaryOp.Add

  //----------------------------------------------------------------------------
  // 11.3.2
  let postDecrement ctx ast = postIncDec ctx ast Ast.BinaryOp.Sub
  
  //----------------------------------------------------------------------------
  // 11.4.1 delete
  let deleteIndex (ctx:Ctx) object' index =
    (Utils.ensureObject ctx object'
      (fun x -> Dlr.call x "Delete" [index])
      (fun x -> Dlr.false'))
    
  let deleteProperty (ctx:Ctx) object' name =
    (Utils.ensureObject ctx object'
      (fun x -> Dlr.call x "Delete" [!!!name])
      (fun x -> Dlr.false'))
    
  let deleteIdentifier (ctx:Ctx) name =
    if ctx.DynamicLookup then
      let args = [ctx.DynamicScope; ctx.Globals; Dlr.const' name]
      Dlr.callStaticT<DynamicScopeHelpers> "Delete" args

    elif Identifier.isGlobal ctx name then
      deleteProperty ctx ctx.Globals name

    else
      Dlr.false'

  let delete (ctx:Ctx) tree =
    match tree with
    | Ast.Identifier name -> 
      deleteIdentifier ctx name

    | Ast.Index(object', index) ->
      let index = Utils.compileIndex ctx index
      let object' = ctx.Compile object'
      deleteIndex ctx object' index

    | Ast.Property(object', name) ->
      let object' = ctx.Compile object'
      deleteProperty ctx object' name

    | _ -> failwith "Que?"

  //----------------------------------------------------------------------------
  // 11.4.2
  let void' (ctx:Ctx) ast =
    Dlr.blockSimple [ctx.Compile ast; Utils.undefined]
        
  //----------------------------------------------------------------------------
  // 11.4.3
  let typeOf (expr:Dlr.Expr) = 
    Operators.typeOf (expr |> Utils.box)
      
  //----------------------------------------------------------------------------
  // 11.4.4, 11.4.5
  let preIncDec (ctx:Ctx) ast op =
    let incrementExpr = 
      ctx.Compile(
        Ast.Assign(ast, 
          Ast.Binary(op, 
            Ast.Convert(TypeTags.Number, ast), 
            Ast.Number 1.0)))

    Dlr.blockSimple [incrementExpr; ctx.Compile ast]
    
  let increment ctx ast = preIncDec ctx ast Ast.BinaryOp.Add
  let decrement ctx ast = preIncDec ctx ast Ast.BinaryOp.Sub
  
  //----------------------------------------------------------------------------
  // 11.4.6
  let plus (ctx:Ctx) ast =
    Dlr.callStaticT<Operators> "plus" [ctx.Compile ast |> Utils.box]

  //----------------------------------------------------------------------------
  // 11.4.7
  let minus (ctx:Ctx) ast =
    Dlr.callStaticT<Operators> "minus" [ctx.Compile ast |> Utils.box]
  
  //----------------------------------------------------------------------------
  // 11.4.8
  let complement (ctx:Ctx) ast =
    Dlr.callStaticT<Operators> "bitCmpl" [ctx.Compile ast |> Utils.box]

  //----------------------------------------------------------------------------
  // 11.4.9
  let not (ctx:Ctx) ast =
    Dlr.callStaticT<Operators> "not" [ctx.Compile ast |> Utils.box]
    
  //----------------------------------------------------------------------------
  let convert (ctx:Ctx) (tag:uint32) (ast:Ast.Tree) =
    match tag with
    | TypeTags.Number ->
      Dlr.callStaticT<TypeConverter> "ToNumber" [ctx.Compile ast]

    | _ -> failwith "Que?"

module Binary = 

  open IronJS.Ast

  //----------------------------------------------------------------------------
  let compileExpr op (l:Dlr.Expr) r =
    match op with
    | BinaryOp.Add -> Operators.add(l, r)
    | BinaryOp.Sub -> Operators.sub(l, r)
    | BinaryOp.Div -> Operators.div(l, r)
    | BinaryOp.Mul -> Operators.mul(l, r)
    | BinaryOp.Mod -> Operators.mod'(l, r)

    | BinaryOp.And -> Operators.and'(l, r)
    | BinaryOp.Or -> Operators.or'(l, r)

    | BinaryOp.BitAnd -> Operators.bitAnd(l, r)
    | BinaryOp.BitOr -> Operators.bitOr(l, r)
    | BinaryOp.BitXor -> Operators.bitXOr(l, r)
    | BinaryOp.BitShiftLeft -> Operators.bitLhs(l, r)
    | BinaryOp.BitShiftRight -> Operators.bitRhs(l, r)
    | BinaryOp.BitUShiftRight -> Operators.bitURhs(l, r)

    | BinaryOp.Eq -> Operators.eq(l, r)
    | BinaryOp.NotEq -> Operators.notEq(l, r)
    | BinaryOp.Same -> Operators.same(l, r)
    | BinaryOp.NotSame -> Operators.notSame(l, r)
    | BinaryOp.Lt -> Operators.lt(l, r)
    | BinaryOp.LtEq -> Operators.ltEq(l, r)
    | BinaryOp.Gt -> Operators.gt(l, r)
    | BinaryOp.GtEq -> Operators.gtEq(l, r)

    | _ -> failwithf "Invalid BinaryOp %A" op
    
  //----------------------------------------------------------------------------
  let compile (ctx:Ctx) op left right =
    let l = ctx.Compile left |> Utils.box 
    let r = ctx.Compile right |> Utils.box
    compileExpr op l r
    
  //----------------------------------------------------------------------------
  let instanceOf (ctx:Context) left right =
    let l = ctx.Compile left |> Utils.box 
    let r = ctx.Compile right |> Utils.box
    Operators.instanceOf(ctx.Env, l, r)
    
  //----------------------------------------------------------------------------
  let in' (ctx:Context) left right =
    let l = ctx.Compile left |> Utils.box 
    let r = ctx.Compile right |> Utils.box
    Operators.in'(ctx.Env, l, r)

  //----------------------------------------------------------------------------
  // 11.13.1 assignment operator =
  let assign (ctx:Context) ltree rtree =
    let value = ctx.Compile rtree

    match ltree with
    //Variable assignment: foo = 1;
    | Ast.Identifier(name) -> 
      Identifier.setValue ctx name value

    //Property assignment: foo.bar = 1;
    | Ast.Property(object', name) -> 
      Utils.blockTmp value (fun value ->
        let object' = object' |> ctx.Compile
        let ifObj = Object.Property.put !!!name value
        let ifClr _ = value
        [Utils.ensureObject ctx object' ifObj ifClr]
      )

    //Index assignemnt: foo[0] = "bar";
    | Ast.Index(object', index) -> 
      Utils.blockTmp value (fun value ->
        let object' = object' |> ctx.Compile
        let index = Utils.compileIndex ctx index
        let ifObj = Object.Index.put index value
        let ifClr _ = value
        [Utils.ensureObject ctx object' ifObj ifClr]
      )

    | _ -> failwithf "Failed to compile assign for: %A" ltree

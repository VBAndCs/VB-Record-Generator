# VB.NET Records:
I don't like how C# designed records. So, I created a demo for an alternative design for records in VB.NET, that gave the same benefits with more flexibility, with no need at all to what is called init-only properties in C#.
This is a WinForms project, where you can write the record code in the generator form, and press the generate button to generate the record class.

# Features:
- The record can be a Class (ref type) or an Structure (value type).
'Public Class Test(X, Y , Z)'
- The record can be mutable, immutable , or in between.
'Friend ReadOnly Class Test(X as integer, Y as String, Z as double)'
- The record class can be inherited.
- The `=` works normally, but the Equality compares the key fields. All fields can be readonly keys (when we use the record keyword):
'record class Test( X as integer, Y = "Test", key Z as double)`
or each individual property can be marked as a key or ReadOnly or both.
`class Test( X as integer, ReadOnly key Y as String = "Test", key Z as double)`
Not that the key and readonly keywords are not new in VB.NET. So, The only thing that I am introducing here is using the record or readonly keywords before the class or struct. This is a minimal non breaking change.
- I am using a static/shared From method to mimic the With expression in C#. `With` already exists in VB before .NET, and the language can allow it to call the From method.
'dim x = Test.From(Y, Name:="Adam", Time:=Now)'
VB can allow a With syntax that exactly calls the From method behind the scene.
'dim x = Y With{.Name = "Adam", .Time = Now}'
Note it is just a fast dirty parser to prove the concept, and it can be easily defied. There is one obvious error in the generated From method, that I left for the user to fix, that is adding `?` after ref types params or the From method. I have no knowledge of types here since this is not a compiler.
- I generate one constructor with required params for readonly properties that have no default values, and optional params for other properties.
Note also that the From method can't set ref types to nothing unless they are nothing in the cloned record.
The generated record contains CType operators to convert between record and tuples in both directions. I generate them for 2-items tuple, 3-Items Typles..... n-Items tuples.
It will be lovely if VB adds deconstruction syntax.


# Syntax variations:
These are some possible variations of syntax. Try them in the generator form:

```vb.net
' immutable but with no keys, will not generate equals methods, so, it is a regular immutable class (or struct if you use Structure) :
Public Readonly class Info(
     X as Integer, 
     Y = "Ali", 
     Z as Date = Now
)
```

```
' Using ReadOnly and Key with members:
Public class Info(
     Key X as Integer, 
     ReadOnly Y = "Ali", 
     ReadOnly key Z as Date = Now
)
```

# A Record Sample:
Try these samples in the generator form:
```vb.net
Public record class Info(
     X as Integer, 
     Y = "Ali", 
     Z as Date = Now
)
```

This is the generated code:
```vb.net
Public Class Info
   Public ReadOnly Property X As Integer
   Public ReadOnly Property Y As Object
   Public ReadOnly Property Z As Date

   Public Sub New(x As Integer, Optional y As Object = "Ali", Optional z As Date = Now)
      _X = X
      _Y = Y
      _Z = Z
   End Sub

   Public Shared Function From(anotherRecord As Info, Optional X As Integer? = Nothing, Optional Y As Object? = Nothing, Optional Z As Date? = Nothing) As Info
      Return New Info(      If(X Is Nothing, anotherRecord.X, X),       If(Y Is Nothing, anotherRecord.Y, Y),       If(Z Is Nothing, anotherRecord.Z, Z))
   End Function

    Public Overrides Function Equals(anotherObject) As Boolean
            Dim anotherRecord = TryCast(anotherObject, Info)
            If anotherRecord Is Nothing Then Return False
            Return Equals(anotherRecord)
        End Function

   Public Overloads Function Equals(anotherRecord As {R.Name}) As Boolean
      If Not X.Equals(anotherRecord.X) Then Return False
      If Not Y.Equals(anotherRecord.Y) Then Return False
      If Not Z.Equals(anotherRecord.Z) Then Return False
      Return True
   End Function

   Public Shared Widening Operator CType(anotherRecord As Info) As (X As Integer, Y As Object)
      Return (anotherRecord.X, anotherRecord.Y)
   End Operator

   Public Shared Widening Operator CType(fromTuple As (X As Integer, Y As Object)) As Info
      Return new Info(fromTuple.X, fromTuple.Y)
   End Operator

   Public Shared Function From(fromTuple As (X As Integer, Y As Object)) As Info
      Return new Info(fromTuple.X, fromTuple.Y)
   End Function

   Public Shared Widening Operator CType(anotherRecord As Info) As (X As Integer, Y As Object, Z As Date)
      Return (anotherRecord.X, anotherRecord.Y, anotherRecord.Z)
   End Operator

   Public Shared Widening Operator CType(fromTuple As (X As Integer, Y As Object, Z As Date)) As Info
      Return new Info(fromTuple.X, fromTuple.Y, fromTuple.Z)
   End Operator

   Public Shared Function From(fromTuple As (X As Integer, Y As Object, Z As Date)) As Info
      Return new Info(fromTuple.X, fromTuple.Y, fromTuple.Z)
   End Function


End  Class
```

# Conclusion:
My question is: why C# complicated it so much, and invented too many new unnecessary concepts, to do so such a simple thing?
I am afraid that C# took a wrong turn since C# 8.0, and can never recover from that!
It is being more gibberish, more complicated, more incomparable with other .NET languages, and way too hard for beginners. Killing VB in such circumstances can make many developers desert .NET. If MS is happy of complicating C#, at least it should let VB.NET as an attractive door for beginners. But note that no one begins with a language without a future, and decays in the market and hiring in companies. On the other hand, if you are shutting down VB, MS must bring more of its spirit to C# to attract its developers and beginners. This is obviously not happening.

' Mohammad Hamdy Ghanem
' Egypt
' 18/11/2020

Imports System.Text
Imports System.Text.RegularExpressions

Public Class Record

    Dim IsImmutable As Boolean
    Dim IsRecord As Boolean
    Dim IsClass As Boolean
    Dim Modifier As String
    Dim Name As String
    Dim Members As New List(Of PropertyInfo)

    Public Shared Function Generate(code As String) As String
        Dim roRegex As New Regex("\breadonly\b", RegexOptions.IgnoreCase)
        Dim keyRegex As New Regex("\bkey\b", RegexOptions.IgnoreCase)
        Dim recordRegex As New Regex("\brecord\b", RegexOptions.IgnoreCase)
        Dim classRegex As New Regex("\bclass|structure\b", RegexOptions.IgnoreCase)
        Dim AsRegex As New Regex("\bas\b", RegexOptions.IgnoreCase)

        code = code.Trim(" "c, vbCr, vbLf)
        Dim R As New Record
        Dim pos = code.IndexOf("("c)
        Dim members = code.Substring(pos + 1, code.Length - pos - 2).Split(",")
        Dim signature = code.Substring(0, pos).Trim(" "c, vbCr, vbLf)
        Dim ro = roRegex.Match(signature)
        R.IsImmutable = ro.Success
        If ro.Success Then signature = signature.Remove(ro.Index, ro.Length)
        Dim record = recordRegex.Match(signature)
        R.IsRecord = record.Success
        If record.Success Then signature = signature.Remove(record.Index, record.Length).Trim(" "c, vbCr, vbLf)

        pos = classRegex.Match(signature).Index
        If signature.ToLower()(pos) = "c"c Then R.IsClass = True

        If pos = 0 Then
            R.Modifier = "Friend"
            R.Name = signature.Trim(" "c, vbCr, vbLf)
        Else
            R.Modifier = signature.Substring(0, pos).Trim(" "c, vbCr, vbLf)
            R.Name = signature.Substring(pos + If(R.IsClass, 5, 9)).Trim(" "c, vbCr, vbLf)
        End If

        For Each member In members
            Dim p As New PropertyInfo
            ro = roRegex.Match(member)
            p.IsReadonly = R.IsImmutable OrElse R.IsRecord OrElse ro.Success
            If ro.Success Then member = member.Remove(ro.Index, ro.Length)
            Dim key = keyRegex.Match(member)
            p.IsKey = R.IsRecord OrElse key.Success
            If key.Success Then member = member.Remove(key.Index, key.Length)

            Dim _as = AsRegex.Match(member)
            If _as.Success Then
                p.Name = member.Substring(0, _as.Index).Trim(" "c, vbCr, vbLf)
                Dim x = member.Substring(_as.Index + 2).Trim(" "c, vbCr, vbLf)
                Dim i = x.IndexOf("=")

                If i > -1 Then
                    p.Type = x.Substring(0, i).Trim(" "c, vbCr, vbLf)
                    p.DefaultValue = x.Substring(i + 1).Trim(" "c, vbCr, vbLf)
                Else
                    p.Type = x.Trim(" "c, vbCr, vbLf)
                End If

            Else
                p.Type = "Object"
                Dim i = member.IndexOf("=")
                If i > -1 Then
                    p.Name = member.Substring(0, i).Trim(" "c, vbCr, vbLf)
                    p.DefaultValue = member.Substring(i + 1).Trim(" "c, vbCr, vbLf)
                Else
                    p.Name = member.Trim(" "c, vbCr, vbLf)
                End If
            End If

            R.Members.Add(p)

        Next

        R.Members = (From m In R.Members
                     Order By m.IsReadonly Descending, m.DefaultValue <> "").ToList

        Dim rcrd As New StringBuilder
        rcrd.Append(R.Modifier)
        rcrd.Append(If(R.IsClass, " Class ", " Structure "))
        rcrd.AppendLine(R.Name)

        ' Properties
        For Each p In R.Members
            rcrd.Append("   Public ")
            If p.IsReadonly Then rcrd.Append("ReadOnly ")
            rcrd.Append($"Property {p.Name} As {p.Type}")
            rcrd.AppendLine()
        Next

        rcrd.AppendLine()

        ' Constructor:
        rcrd.Append("   Public Sub New(")
        Dim body As New StringBuilder
        Dim AddSep = False

        For Each p In R.Members
            Dim isOptional = Not p.IsReadonly Or p.DefaultValue <> ""
            If AddSep Then rcrd.Append(", ")
            If isOptional Then rcrd.Append("Optional ")
            rcrd.Append(p.Name.Substring(0, 1).ToLower())
            rcrd.Append(p.Name.Substring(1))
            rcrd.Append($" As {p.Type}")
            If isOptional Then
                rcrd.Append(" = ")
                rcrd.Append(If(p.DefaultValue = "", "Nothing", p.DefaultValue))
            End If
            AddSep = True
            body.AppendLine($"      _{p.Name} = {p.Name}")
        Next
        rcrd.AppendLine(")")
        rcrd.Append(body.ToString())
        rcrd.AppendLine("   End Sub")

        rcrd.AppendLine()

        ' From
        body.Clear()
        rcrd.Append("   Public Shared Function From(anotherRecord As ")
        rcrd.Append(R.Name)
        AddSep = False
        For Each p In R.Members
            rcrd.Append($", Optional {p.Name} As {p.Type}? = Nothing")
            If AddSep Then body.Append(", ")
            body.Append($"      If({p.Name} Is Nothing, anotherRecord.{p.Name}, {p.Name})")
            AddSep = True
        Next
        rcrd.AppendLine($") As {R.Name}")
        rcrd.Append($"      Return New {R.Name}(")
        rcrd.Append(body.ToString())
        rcrd.AppendLine(")")
        rcrd.AppendLine("   End Function")

        rcrd.AppendLine()
        ' Equals
        Dim keys = From m In R.Members
                   Where m.IsKey

        If keys.Any Then
            rcrd.AppendLine($"    Public Overrides Function Equals(anotherObject) As Boolean
            Dim anotherRecord = TryCast(anotherObject, {R.Name})
            If anotherRecord Is Nothing Then Return False
            Return Equals(anotherRecord)
        End Function")
            rcrd.AppendLine()

            rcrd.AppendLine("   Public Overloads Function Equals(anotherRecord As {R.Name}) As Boolean")
            For Each p In keys
                rcrd.AppendLine($"      If Not {p.Name}.Equals(anotherRecord.{p.Name}) Then Return False")
            Next
            rcrd.AppendLine("      Return True")
            rcrd.AppendLine("   End Function")
        End If

        rcrd.AppendLine()
        Dim requireed = Aggregate m In R.Members
                        Where m.IsReadonly And m.DefaultValue = ""
                            Into Count

        ' Tuples
        For n = Math.Max(requireed, 1) To R.Members.Count - 1
            body.Clear()
            AddSep = False
            rcrd.Append($"   Public Shared Widening Operator CType(anotherRecord As {R.Name}) As (")
            For i = 0 To n
                Dim p = R.Members(i)
                If AddSep Then
                    rcrd.Append(", ")
                    body.Append(", ")
                End If
                rcrd.Append($"{p.Name} As {p.Type}")
                body.Append($"anotherRecord.{p.Name}")
                AddSep = True
            Next
            rcrd.AppendLine(")")
            rcrd.Append("      Return (")
            rcrd.Append(body.ToString)
            rcrd.AppendLine(")")
            rcrd.AppendLine("   End Operator")
            rcrd.AppendLine()

            rcrd.Append($"   Public Shared Widening Operator CType(fromTuple As (")
            Dim methodType = "Operator"
LineAgain:
            body.Clear()
            AddSep = False

            For i = 0 To n
                Dim p = R.Members(i)
                If AddSep Then
                    rcrd.Append(", ")
                    body.Append(", ")
                End If
                rcrd.Append($"{p.Name} As {p.Type}")
                body.Append($"fromTuple.{p.Name}")
                AddSep = True
            Next
            rcrd.AppendLine($")) As {R.Name}")
            rcrd.Append($"      Return new {R.Name}(")
            rcrd.Append(body.ToString)
            rcrd.AppendLine(")")
            rcrd.AppendLine($"   End {MethodType}")
            rcrd.AppendLine()

            If methodType = "Operator" Then
                methodType = "Function"
                rcrd.Append($"   Public Shared Function From(fromTuple As (")
                GoTo LineAgain
            End If
        Next

        rcrd.AppendLine()
        rcrd.Append("End ")
        rcrd.Append(If(R.IsClass, " Class", " Structure"))

        Return rcrd.ToString()
    End Function
End Class

Friend Class PropertyInfo
    Public IsReadonly As Boolean
    Public IsKey As Boolean
    Public Name As String
    Public Type As String
    Public DefaultValue As String
End Class

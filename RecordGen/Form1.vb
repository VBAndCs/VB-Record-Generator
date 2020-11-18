Imports System.Linq.Expressions

Public Class Form1
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim saveFile As New SaveFileDialog
        saveFile.Filter = "vb files|*.vb"
        saveFile.OverwritePrompt = True
        saveFile.FileName = txtRecord.Text & ".gen.vb"
        If saveFile.ShowDialog() = DialogResult.OK Then
            IO.File.WriteAllText(saveFile.FileName, txtResult.Text)
        End If
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        txtResult.Text = Record.Generate(txtRecord.Text)

    End Sub

        Private Function GenerateRecord(text As String, exp As Expression) As String
            Throw New NotImplementedException()
        End Function
    End Class

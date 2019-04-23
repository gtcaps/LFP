Public Class AcercaDe
    Public Property Form1 As Form1
        Get
            Return Nothing
        End Get
        Set(value As Form1)
        End Set
    End Property

    Private Sub Regresar_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles Regresar.LinkClicked
        Me.Close()
        Form1.Show()
    End Sub
End Class
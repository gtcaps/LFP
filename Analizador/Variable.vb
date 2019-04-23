Public Class Variable

    Private nombre As String
    Private valor
    Private tipo As String

    Public Sub New(nombre As String, tipo As String)
        Me.nombre = nombre
        Me.valor = ""
        Me.tipo = tipo
    End Sub

    Public Sub New(nombre As String, valor As String, tipo As String)
        Me.nombre = nombre
        Me.valor = valor
        Me.tipo = tipo
    End Sub

    Public Sub New(nombre As String, valor As Integer, tipo As String)
        Me.nombre = nombre
        Me.valor = valor
        Me.tipo = tipo
    End Sub

    Public Property Form1 As Form1
        Get
            Return Nothing
        End Get
        Set(value As Form1)
        End Set
    End Property

    Public Function getNombre() As String
        Return Me.nombre
    End Function

    Public Function getValor() As String
        Return Me.valor
    End Function

    Public Function getTipo() As String
        Return Me.tipo
    End Function

    Public Sub setValor(valor As String)
        Me.valor = valor
    End Sub


End Class

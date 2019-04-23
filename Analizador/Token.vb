Public Class Token

    Private tipo As String
    Private lexema As String
    Private linea As Integer
    Private columna As Integer

    Public Sub New()
        tipo = ""
        lexema = ""
        linea = 0
        columna = 0
    End Sub

    Public Sub New(tipo As String, lexema As String, linea As Integer, columna As Integer)
        Me.tipo = tipo
        Me.lexema = lexema
        Me.linea = linea
        Me.columna = columna
    End Sub

    Public Property Form1 As Form1
        Get
            Return Nothing
        End Get
        Set(value As Form1)
        End Set
    End Property

    Function getTipo() As String
        Return tipo
    End Function

    Function getlexema() As String
        Return lexema
    End Function

    Function getLinea() As Integer
        Return linea
    End Function

    Function getcolumna() As Integer
        Return columna
    End Function

End Class

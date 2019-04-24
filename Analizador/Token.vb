Public Class Token

    Private tipoT As Tipo
    Private lexema As String
    Private linea As Integer
    Private columna As Integer

    Enum Tipo
        ID = 100
        PALABRA_RESERVADA = 101
        CADENA_DE_TEXTO = 102
        NUMERO_ENTERO = 103
        NUMERO_DECIMAL = 104
        LLAVE_IZQ = 105
        LLAVE_DER = 106
        PUNTO_Y_COMA = 107
        PARENTESIS_IZQ = 108
        PARENTESIS_DER = 109
        CORCHETE_IZQ = 110
        CORCHETE_DER = 111
        DOS_PUNTOS = 112
        ASTERISCO = 113
        COMILLAS_DOBLE = 114
        MAS = 115
        DIVISION = 116
        COMA = 117
        IGUAL = 118
        GUION_BAJO = 119
        PUNTO = 120
        GUION_MEDIO = 121
        ERROR_LEXICO = 122
    End Enum

    Public Sub New()
        lexema = ""
        linea = 0
        columna = 0
    End Sub

    Public Sub New(tipo As Tipo, lexema As String, linea As Integer, columna As Integer)
        Me.tipoT = tipo
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
        Return tipoT
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

Imports Analizador.Token
Imports Analizador.Form1

Public Class AnalizadorSintactico

    Dim numPreanalisis As Integer
    Dim preAnalisis As Token
    Dim listaTokens As ArrayList

    Public Sub parsear(listaToken As ArrayList)
        Me.listaTokens = listaToken
        preAnalisis = listaTokens.Item(0)
        numPreanalisis = 0
        BLOQUE()
    End Sub

    Private Sub BLOQUE()
        If (preAnalisis.getlexema.Trim.Equals("instrucciones")) Then
            match(preAnalisis.getTipo)
            match(Tipo.LLAVE_IZQ)
            FUNCIONES_INSTRUCCIONES()
            match(Tipo.LLAVE_DER)
            BLOQUE_P()
        ElseIf (preAnalisis.getlexema.Trim.Equals("variables")) Then
        ElseIf (preAnalisis.getlexema.Trim.Equals("texto")) Then
        End If
    End Sub

    Private Sub BLOQUE_P()
        If (preAnalisis.getlexema.Trim.Equals("instrucciones")) Then
            BLOQUE()
        ElseIf (preAnalisis.getlexema.Trim.Equals("variables")) Then
            BLOQUE()
        ElseIf (preAnalisis.getlexema.Trim.Equals("texto")) Then
            BLOQUE()
        End If
    End Sub

    Private Sub FUNCIONES_INSTRUCCIONES()
        If (preAnalisis.getlexema.Trim.Equals("interlineado")) Then
            match(preAnalisis.getTipo)
            match(Tipo.PARENTESIS_IZQ)
            match(Tipo.NUMERO_DECIMAL)
            match(Tipo.PARENTESIS_DER)
            match(Tipo.PUNTO_Y_COMA)
            FUNCIONES_INSTRUCCIONES()
        ElseIf (preAnalisis.getlexema.Trim.Equals("tamanio_letra")) Then
            match(preAnalisis.getTipo)
            match(Tipo.PARENTESIS_IZQ)
            match(Tipo.NUMERO_ENTERO)
            match(Tipo.PARENTESIS_DER)
            match(Tipo.PUNTO_Y_COMA)
            FUNCIONES_INSTRUCCIONES()
        ElseIf (preAnalisis.getlexema.Trim.Equals("nombre_archivo")) Then
            match(preAnalisis.getTipo)
            match(Tipo.PARENTESIS_IZQ)
            match(Tipo.CADENA_DE_TEXTO)
            match(Tipo.PARENTESIS_DER)
            match(Tipo.PUNTO_Y_COMA)
            FUNCIONES_INSTRUCCIONES()
        ElseIf (preAnalisis.getlexema.Trim.Equals("direccion_archivo")) Then
            match(preAnalisis.getTipo)
            match(Tipo.PARENTESIS_IZQ)
            match(Tipo.CADENA_DE_TEXTO)
            match(Tipo.PARENTESIS_DER)
            match(Tipo.PUNTO_Y_COMA)
            FUNCIONES_INSTRUCCIONES()
        End If
    End Sub

    Private Sub match(p As Tipo)
        If Not p = preAnalisis.getTipo Then
            Form1.listaErrores.add("Se esperaba " + obtenerError(p) + " en la linea " + listaTokens.Item(numPreanalisis - 1).getLinea.ToString)
            numPreanalisis -= 1
        End If
        If Not preAnalisis.getTipo = Tipo.ERROR_LEXICO Then
            numPreanalisis += 1
            preAnalisis = listaTokens.Item(numPreanalisis)
        End If
    End Sub

    Private Function obtenerError(p As Tipo) As String
        Select Case p
            Case (Tipo.ID)
                Return "Identificador"
            Case (Tipo.PALABRA_RESERVADA)
                Return "Palabra Reservada"
            Case (Tipo.CADENA_DE_TEXTO)
                Return "Cadena de Texto"
            Case (Tipo.NUMERO_ENTERO)
                Return "# Entero"
            Case (Tipo.NUMERO_DECIMAL)
                Return "# Decimal"
            Case (Tipo.LLAVE_IZQ)
                Return "Simbolo {"
            Case (Tipo.LLAVE_DER)
                Return "Simbolo }"
            Case (Tipo.PUNTO_Y_COMA)
                Return "Simbolo ;"
            Case (Tipo.PARENTESIS_IZQ)
                Return "Simbolo ("
            Case (Tipo.PARENTESIS_DER)
                Return "Simbolo )"
            Case (Tipo.CORCHETE_IZQ)
                Return "Simbolo ["
            Case (Tipo.CORCHETE_DER)
                Return "Simbolo ]"
            Case (Tipo.DOS_PUNTOS)
                Return "Simbolo :"
            Case (Tipo.ASTERISCO)
                Return "Simbolo *"
            Case (Tipo.COMILLAS_DOBLE)
                Return "Simbolo " + Chr(34).ToString
            Case (Tipo.MAS)
                Return "Simbolo +"
            Case (Tipo.DIVISION)
                Return " Simbolo /"
            Case (Tipo.COMA)
                Return "Simbolo ,"
            Case (Tipo.IGUAL)
                Return "Simbolo ="
            Case (Tipo.GUION_BAJO)
                Return "Simbolo _"
            Case (Tipo.PUNTO)
                Return "Simbolo ."
            Case (Tipo.GUION_MEDIO)
                Return "Simbolo -"
            Case Else
                Return "ERROR LEXICO"
        End Select
    End Function

End Class

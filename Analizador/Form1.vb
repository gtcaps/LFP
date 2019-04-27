Imports linq
Imports iTextSharp
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports System.IO
Imports Analizador.Token

Public Class Form1

    'LISTAS PARA GUARDAR LOS TOKENS
    Dim listaTokensValidos As New ArrayList
    Dim listaTokensErroneos As New ArrayList
    Public listaErrores = New ArrayList
    Dim listaSimbolos() As Integer = {Asc("{"), Asc("}"), Asc(";"), Asc("("), Asc(")"), Asc("["), Asc("]"),
        Asc(":"), Asc("*"), 34, Asc("+"), Asc("/"), Asc(","), Asc("="), Asc("_"), Asc("."), Asc("-")}
    Dim palabrasReservadas() As String = {"instrucciones", "variables", "texto", "interlineado",
        "tamanio_letra", "nombre_archivo", "direccion_archivo", "cadena", "entero", "imagen", "numeros",
        "linea_en_blanco", "var", "promedio", "suma", "asignar", "resta", "multiplicar", "division"}

    Dim listaInstrucciones = New ArrayList
    Dim listaTexto = New ArrayList
    Dim listaVariables = New ArrayList
    Dim listaComentarios = New ArrayList

    'ESPECIFICACIONES GENERALES DEL ARCHIVO DE SALIDA
    Dim interlineadoDefault = 1.5, tamanioLetraDefault = 11, direccionArchivoDefault = "C:\pdf"
    Dim nombreArchivoDefault = "salida.pdf"
    Dim interlineado = interlineadoDefault
    Dim tamanioLetra = tamanioLetraDefault
    Dim direccionArchivo = direccionArchivoDefault
    Dim nombreArchivoS = nombreArchivoDefault



    '------------------------------------------------------------
    '   ANALIZADOR LEXICO 
    '------------------------------------------------------------
    Sub analizar(cadena As String, fila As Integer)
        Dim estadoActual = 0
        Dim texto = ""
        Dim c = cadena.ToCharArray
        Dim col = 0
        Dim columna = 0
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = c.Length


        '------------------------------------------------------------
        '   For que recorre los caracteres
        '------------------------------------------------------------
        For i = 0 To (c.Length - 1)
            col = col + 1
            'PROGRESSBAR
            If ProgressBar1.Value <= ProgressBar1.Maximum - 1 Then
                TextBox2.AppendText(c.GetValue(i))
                ProgressBar1.Value += 1
            End If

            '------------------------------------------------------------
            '   Estado Inicial 
            '------------------------------------------------------------
            If (estadoActual = 0) Then
                columna = col
                '------------------------------------------------------------
                '   Si el primer caracter es una letra 
                '------------------------------------------------------------
                If (Char.IsLetter(c.GetValue(i))) Then 'LETRA
                    texto = texto + c.GetValue(i)
                    estadoActual = 1
                    Continue For
                    '------------------------------------------------------------
                    '   Si el primer Caracter un Digito 
                    '------------------------------------------------------------
                ElseIf (Char.IsDigit(c.GetValue(i))) Then
                    texto = texto + c.GetValue(i)
                    estadoActual = 4
                    Continue For
                    '------------------------------------------------------------
                    '   Si Pertenece a la lista de simbolos 
                    '------------------------------------------------------------
                ElseIf (listaSimbolos.Contains(Asc(c.GetValue(i)))) Then
                    '------------------------------------------------------------
                    '   Simbolo " 
                    '------------------------------------------------------------
                    If (Asc(c.GetValue(i)) = 34) Then
                        estadoActual = 3
                        texto = texto + c.GetValue(i)
                        Continue For
                        '------------------------------------------------------------
                        '   Simbolo - 
                        '------------------------------------------------------------
                    ElseIf (Asc(c.GetValue(i)) = Asc("-")) Then
                        estadoActual = 4
                        texto = texto + c.GetValue(i)
                        Continue For
                        '------------------------------------------------------------
                        '   Cualquier Simbolo de la lista 
                        '------------------------------------------------------------
                    Else
                        listaTokensValidos.Add(New Token(tipoSimbolo(c(i).ToString.Trim), c.GetValue(i), fila, columna))
                        Continue For
                    End If
                    '------------------------------------------------------------
                    '   Si se encuentra un espacio en blanco 
                    '------------------------------------------------------------
                Else
                    If (Char.IsWhiteSpace(c.GetValue(i))) Then
                        '------------------------------------------------------------
                        '   Si el caracter no pertenece al lenguaje 
                        '------------------------------------------------------------
                    Else
                        listaTokensErroneos.Add(New Token("CARACTER DESCONOCIDO ", c.GetValue(i), fila, columna))
                    End If
                End If
            End If

            '------------------------------------------------------------
            '   Estado 1 
            '------------------------------------------------------------
            If (estadoActual = 1) Then
                If (Char.IsLetterOrDigit(c.GetValue(i))) Then 'LETRA O DIGITO
                    texto = texto + c.GetValue(i).ToString
                    estadoActual = 1
                    Continue For
                ElseIf (listaSimbolos.Contains(Asc(c.GetValue(i)))) Then 'GUION BAJO EN UNA PALABRA                    
                    If (Char.ToString(c.GetValue(i)).Equals("_")) Then
                        texto = texto + c.GetValue(i).ToString
                        estadoActual = 2
                        Continue For
                    ElseIf (Asc(c.GetValue(i)) = 34) Then
                        If (palabrasReservadas.Contains(texto)) Then
                            listaTokensValidos.Add(New Token(Tipo.PALABRA_RESERVADA, texto, fila, columna))
                        Else
                            listaTokensValidos.Add(New Token(Tipo.ID, texto, fila, columna))
                        End If
                        texto = ""
                        estadoActual = 3
                        Continue For
                    ElseIf (Asc(c.GetValue(i)) = Asc("-")) Then
                        If (palabrasReservadas.Contains(texto)) Then
                            listaTokensValidos.Add(New Token(Tipo.PALABRA_RESERVADA, texto, fila, columna))
                        Else
                            listaTokensValidos.Add(New Token(Tipo.ID, texto, 0, 0))
                        End If
                        texto = ""
                        estadoActual = 4
                        texto = texto + c.GetValue(i)
                        Continue For
                    Else
                        If (palabrasReservadas.Contains(texto)) Then
                            listaTokensValidos.Add(New Token(Tipo.PALABRA_RESERVADA, texto, fila, columna))
                        Else
                            listaTokensValidos.Add(New Token(Tipo.ID, texto, fila, columna))
                        End If
                        listaTokensValidos.Add(New Token(tipoSimbolo(c(i).ToString.Trim), c.GetValue(i), fila, columna))
                        texto = ""
                        estadoActual = 0
                        Continue For
                    End If
                ElseIf (Char.IsWhiteSpace(c.GetValue(i)) Or i = (c.Length - 1)) Then 'ESPACIO EN BLANCO
                    If (palabrasReservadas.Contains(texto)) Then
                        listaTokensValidos.Add(New Token(Tipo.PALABRA_RESERVADA, texto, fila, columna))
                    Else
                        listaTokensValidos.Add(New Token(Tipo.ID, texto, fila, columna))
                    End If
                    texto = ""
                    estadoActual = 0
                    Continue For

                End If
            End If

            '------------------------------------------------------------
            '   Estado 2 
            '------------------------------------------------------------
            If (estadoActual = 2) Then
                If (Char.IsLetter(c.GetValue(i))) Then 'texto = texto + c.GetValue(i).ToString
                    texto = texto + c.GetValue(i).ToString
                    estadoActual = 2
                    Continue For
                ElseIf (Char.ToString(c.GetValue(i)).Equals("_")) Then
                    texto = texto + c.GetValue(i).ToString
                    estadoActual = 2
                    Continue For
                ElseIf (listaSimbolos.Contains(Asc(c.GetValue(i)))) Then
                    If (palabrasReservadas.Contains(texto)) Then
                        listaTokensValidos.Add(New Token(Tipo.PALABRA_RESERVADA, texto, fila, columna))
                    Else
                        listaTokensValidos.Add(New Token(Tipo.ID, texto, fila, columna))
                    End If
                    listaTokensValidos.Add(New Token(tipoSimbolo(c(i).ToString.Trim), c.GetValue(i), fila, columna))
                    texto = ""
                    estadoActual = 0
                    Continue For
                ElseIf (Char.IsWhiteSpace(c.GetValue(i)) Or Char.ToString(c.GetValue(i)).Equals("#")) Then 'ESPACIO EN BLANCO
                    If (palabrasReservadas.Contains(texto)) Then
                        listaTokensValidos.Add(New Token(Tipo.PALABRA_RESERVADA, texto, fila, columna))
                    Else
                        listaTokensValidos.Add(New Token(Tipo.ID, texto, fila, columna))
                    End If
                    texto = ""
                    estadoActual = 0
                    Continue For
                End If
            End If

            '------------------------------------------------------------
            '   Estado 3 
            '------------------------------------------------------------ 
            If (estadoActual = 3) Then 'texto = texto + c.GetValue(i).ToString
                If (Asc(c.GetValue(i)) = 34) Then
                    texto = texto + c.GetValue(i).ToString
                    listaTokensValidos.Add(New Token(Tipo.CADENA_DE_TEXTO, texto, fila, columna))
                    texto = ""
                    estadoActual = 0
                    Continue For
                Else
                    texto = texto + c.GetValue(i).ToString
                End If
            End If

            '------------------------------------------------------------
            '   Estado 4 
            '------------------------------------------------------------
            If (estadoActual = 4) Then
                If (Char.IsDigit(c.GetValue(i))) Then
                    texto = texto + c.GetValue(i).ToString
                    estadoActual = 4
                    Continue For
                ElseIf (Char.IsLetter(c.GetValue(i))) Then 'texto = texto + c.GetValue(i).ToString
                    listaTokensValidos.Add(New Token(tipoSimbolo(texto.ToString.Trim), texto, fila, columna))
                    texto = ""
                    texto = texto + c.GetValue(i)
                    estadoActual = 2
                    Continue For
                ElseIf (listaSimbolos.Contains(Asc(c.GetValue(i)))) Then 'GUION BAJO EN UNA PALABRA 
                    If (Char.ToString(c.GetValue(i)).Equals(".")) Then
                        texto = texto + c.GetValue(i).ToString
                        estadoActual = 5
                        Continue For
                    ElseIf (Asc(c.GetValue(i)) = 34) Then
                        listaTokensValidos.Add(New Token(Tipo.NUMERO_ENTERO, texto, fila, columna))
                        texto = ""
                        estadoActual = 3
                        Continue For
                    Else
                        listaTokensValidos.Add(New Token(Tipo.NUMERO_ENTERO, texto, 0, 0))
                        listaTokensValidos.Add(New Token(tipoSimbolo(c(i).ToString.Trim), c.GetValue(i), fila, columna))
                        texto = ""
                        estadoActual = 0
                        Continue For
                    End If
                ElseIf (Char.IsWhiteSpace(c.GetValue(i))) Then 'ESPACIO EN BLANCO
                    listaTokensValidos.Add(New Token(Tipo.NUMERO_ENTERO, texto, fila, columna))
                    texto = ""
                    estadoActual = 0
                    Continue For
                End If
            End If

            '------------------------------------------------------------
            '   Estado 5 
            '------------------------------------------------------------
            If (estadoActual = 5) Then
                If (Char.IsDigit(c.GetValue(i))) Then
                    texto = texto + c.GetValue(i).ToString
                    estadoActual = 5
                    Continue For
                ElseIf (listaSimbolos.Contains(Asc(c.GetValue(i)))) Then 'GUION BAJO EN UNA PALABRA 
                    If (Char.ToString(c.GetValue(i)).Equals(".")) Then
                        texto = texto + c.GetValue(i).ToString
                        estadoActual = 5
                        Continue For
                    ElseIf (Asc(c.GetValue(i)) = 34) Then
                        Dim n As Integer = (From cr In texto Where cr = "." Select cr).Count
                        If (n > 1) Then
                            listaTokensErroneos.Add(New Token("ERROR NUMERO DECIMAL", texto, fila, columna))
                        Else
                            listaTokensValidos.Add(New Token(Tipo.NUMERO_DECIMAL, texto, fila, columna))
                        End If
                        texto = ""
                        estadoActual = 3
                        Continue For
                    Else
                        Dim n As Integer = (From cr In texto Where cr = "." Select cr).Count
                        If (n > 1) Then
                            listaTokensErroneos.Add(New Token("ERROR NUMERO DECIMAL", texto, fila, columna))
                        Else
                            listaTokensValidos.Add(New Token(Tipo.NUMERO_DECIMAL, texto, fila, columna))
                        End If
                        listaTokensValidos.Add(New Token(tipoSimbolo(c(i).ToString.Trim), c.GetValue(i), fila, columna))
                        texto = ""
                        estadoActual = 0
                        Continue For
                    End If
                ElseIf (Char.IsWhiteSpace(c.GetValue(i)) Or Char.ToString(c.GetValue(i)).Equals("#")) Then 'ESPACIO EN BLANCO
                    Dim n As Integer = (From cr In texto Where cr = "." Select cr).Count
                    If (n > 1) Then
                        listaTokensErroneos.Add(New Token("ERROR NUMERO DECIMAL", texto, fila, columna))
                    Else
                        listaTokensValidos.Add(New Token(Tipo.NUMERO_DECIMAL, texto, fila, columna))
                    End If
                    texto = ""
                    estadoActual = 0
                    Continue For
                End If
            End If

        Next


    End Sub

    Private Function tipoSimbolo(simbolo As String) As Tipo
        If (simbolo.Equals("{")) Then
            Return Tipo.LLAVE_IZQ
        ElseIf (simbolo.Equals("}")) Then
            Return Tipo.LLAVE_DER
        ElseIf (simbolo.Equals(";")) Then
            Return Tipo.PUNTO_Y_COMA
        ElseIf (simbolo.Equals("(")) Then
            Return Tipo.PARENTESIS_IZQ
        ElseIf (simbolo.Equals(")")) Then
            Return Tipo.PARENTESIS_DER
        ElseIf (simbolo.Equals("[")) Then
            Return Tipo.CORCHETE_IZQ
        ElseIf (simbolo.Equals("]")) Then
            Return Tipo.CORCHETE_DER
        ElseIf (simbolo.Equals(":")) Then
            Return Tipo.DOS_PUNTOS
        ElseIf (simbolo.Equals("*")) Then
            Return Tipo.ASTERISCO
        ElseIf (simbolo.Equals(Chr(34))) Then
            Return Tipo.COMILLAS_DOBLE
        ElseIf (simbolo.Equals("+")) Then
            Return Tipo.MAS
        ElseIf (simbolo.Equals("/")) Then
            Return Tipo.DIVISION
        ElseIf (simbolo.Equals(",")) Then
            Return Tipo.COMA
        ElseIf (simbolo.Equals("=")) Then
            Return Tipo.IGUAL
        ElseIf (simbolo.Equals("_")) Then
            Return Tipo.GUION_BAJO
        ElseIf (simbolo.Equals(".")) Then
            Return Tipo.PUNTO
        ElseIf (simbolo.Equals("-")) Then
            Return Tipo.GUION_MEDIO
        Else
            Return Tipo.ERROR_LEXICO
        End If
    End Function

    Private Function nombreSimbolo(tipo As Tipo) As String
        If (tipo = Tipo.ID) Then
            Return "Identificador"
        ElseIf (tipo = Tipo.PALABRA_RESERVADA) Then
            Return "Palabra Reservada"
        ElseIf (tipo = Tipo.CADENA_DE_TEXTO) Then
            Return "Cadena de Texto"
        ElseIf (tipo = Tipo.NUMERO_ENTERO) Then
            Return "# Entero"
        ElseIf (tipo = Tipo.NUMERO_DECIMAL) Then
            Return "# Decimal"
        ElseIf (tipo = Tipo.LLAVE_IZQ) Then
            Return "Simbolo {"
        ElseIf (tipo = Tipo.LLAVE_DER) Then
            Return "Simbolo }"
        ElseIf (tipo = Tipo.PUNTO_Y_COMA) Then
            Return "Simbolo ;"
        ElseIf (tipo = Tipo.PARENTESIS_IZQ) Then
            Return "Simbolo ("
        ElseIf (tipo = Tipo.PARENTESIS_DER) Then
            Return "Simbolo )"
        ElseIf (tipo = Tipo.CORCHETE_IZQ) Then
            Return "Simbolo ["
        ElseIf (tipo = Tipo.CORCHETE_DER) Then
            Return "Simbolo ]"
        ElseIf (tipo = Tipo.DOS_PUNTOS) Then
            Return "Simbolo :"
        ElseIf (tipo = Tipo.ASTERISCO) Then
            Return "Simbolo *"
        ElseIf (tipo = Tipo.COMILLAS_DOBLE) Then
            Return "Simbolo " + Chr(34).ToString
        ElseIf (tipo = Tipo.MAS) Then
            Return "Simbolo +"
        ElseIf (tipo = Tipo.DIVISION) Then
            Return " Simbolo /"
        ElseIf (tipo = Tipo.COMA) Then
            Return "Simbolo ,"
        ElseIf (tipo = Tipo.IGUAL) Then
            Return "Simbolo ="
        ElseIf (tipo = Tipo.GUION_BAJO) Then
            Return "Simbolo _"
        ElseIf (tipo = Tipo.PUNTO) Then
            Return "Simbolo ."
        ElseIf (tipo = Tipo.GUION_MEDIO) Then
            Return "Simbolo -"
        Else
            Return "ERROR LEXICO"
        End If
    End Function


    '------------------------------------------------------------
    '   REPORTE 
    '------------------------------------------------------------
    Private Sub generarReportesPDF(ruta As String, nombreArchivo As String, lista As ArrayList, nReporte As String)


        Try
            '------------------------------------------------------------
            '   Configuraciones Iniciales
            '------------------------------------------------------------
            Dim pdfDoc As New Document(PageSize.A4, 30.0F, 30.0F, 30.0F, 30.0F)
            Dim pdfWrite As PdfWriter = PdfWriter.GetInstance(pdfDoc, New FileStream(ruta + "\" + nombreArchivo, FileMode.Create))

            '------------------------------------------------------------
            '   Se Abre el Documento
            '------------------------------------------------------------
            pdfDoc.Open()

            '------------------------------------------------------------
            '   Encabezado de la Universidad
            '------------------------------------------------------------
            Dim negrita As Font = FontFactory.GetFont("Verdana", 12, 1)
            pdfDoc.Add(New Paragraph("UNIVERSIDAD DE SAN CARLOS DE GUATEMALA", negrita))
            pdfDoc.Add(New Paragraph("FACULTAD DE INGENIERIA", negrita))
            pdfDoc.Add(New Paragraph("ESCUELA DE CIENCIAS", negrita))
            pdfDoc.Add(New Paragraph("INGENIERIA EN CIENCIAS Y SISTEMAS", negrita))
            pdfDoc.Add(New Paragraph("LENGUAJES FORMALES Y DE PROGRAMACION", negrita))

            '------------------------------------------------------------
            '   Imagen 
            '------------------------------------------------------------
            Dim urlLogo = Application.StartupPath + "\Resources\logo.png"
            If (System.IO.File.Exists(urlLogo)) Then
                'AGREGAMOS LA IMAGEN AL DOCUMENTO Y LA CONFIGURAMOS
                Dim logoUsac = iTextSharp.text.Image.GetInstance(urlLogo)
                logoUsac.ScaleToFit(140.0F, 120.0F)
                logoUsac.SetAbsolutePosition(450, 700)
                pdfDoc.Add(logoUsac)
            End If

            '------------------------------------------------------------
            '   titulo Reportes
            '------------------------------------------------------------
            pdfDoc.Add(New Chunk(vbNewLine))
            Dim fT As Font = FontFactory.GetFont("Arial", 18, 1)
            Dim titulo = New Paragraph("REPORTE " + nReporte.ToUpper, fT)
            titulo.Alignment = Element.ALIGN_CENTER
            pdfDoc.Add(titulo)

            '------------------------------------------------------------
            '   Encabezado de los archivos
            '------------------------------------------------------------

            Dim negrita2 As Font = FontFactory.GetFont("Verdana", 11.5, 1)
            pdfDoc.Add(New Chunk("Archivo Fuente: ", negrita2))
            pdfDoc.Add(New Chunk(lblNomArchivo.Text.ToString + vbNewLine))
            pdfDoc.Add(New Chunk("Archivo Salida: ", negrita2))
            pdfDoc.Add(New Chunk(nombreArchivoS.ToString.ToUpper))

            '------------------------------------------------------------
            '   Generación Tabla
            '------------------------------------------------------------
            Dim table As PdfPTable = New PdfPTable(5)
            Dim ancho() = {25, 285, 100, 35, 55}
            table.TotalWidth = 500.0F
            table.LockedWidth = True
            table.SetWidths(ancho)

            Dim fEnc As Font = FontFactory.GetFont("Verdana", 10, 1)
            fEnc.Color = iTextSharp.text.pdf.ExtendedColor.WHITE
            Dim c1 = New PdfPCell(New Phrase("No", fEnc))
            Dim c2 = New PdfPCell(New Phrase("Lexema", fEnc))
            Dim c3 = New PdfPCell(New Phrase("Tipo", fEnc))
            Dim c4 = New PdfPCell(New Phrase("Fila", fEnc))
            Dim c5 = New PdfPCell(New Phrase("Columna", fEnc))

            c1.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c2.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c3.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c4.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c5.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK

            c1.HorizontalAlignment = 1
            c2.HorizontalAlignment = 1
            c3.HorizontalAlignment = 1
            c4.HorizontalAlignment = 1
            c5.HorizontalAlignment = 1

            c1.Padding = 5
            c2.Padding = 5
            c3.Padding = 5
            c4.Padding = 5
            c5.Padding = 5

            table.AddCell(c1)
            table.AddCell(c2)
            table.AddCell(c3)
            table.AddCell(c4)
            table.AddCell(c5)


            '------------------------------------------------------------
            '   Llenar la tabla con los datos
            '------------------------------------------------------------
            Dim c = 0
            Dim fDig As Font = FontFactory.GetFont("Verdana", 10)

            If lista.Count <> 0 Then
                For Each ls In lista
                    Dim ls1 = New PdfPCell(New Phrase(c, fDig))
                    Dim ls2 = New PdfPCell(New Phrase(ls.getLexema, fDig))
                    Dim ls3 = New PdfPCell(New Phrase(nombreSimbolo(ls.getTipo), fDig))
                    Dim ls4 = New PdfPCell(New Phrase(ls.getLinea, fDig))
                    Dim ls5 = New PdfPCell(New Phrase(ls.getColumna, fDig))

                    ls1.Padding = 3
                    ls2.Padding = 3
                    ls3.Padding = 3
                    ls4.Padding = 3
                    ls5.Padding = 3

                    ls1.FixedHeight = 5.0F

                    ls1.HorizontalAlignment = 1
                    ls4.HorizontalAlignment = 1
                    ls5.HorizontalAlignment = 1

                    table.AddCell(ls1)
                    table.AddCell(ls2)
                    table.AddCell(ls3)
                    table.AddCell(ls4)
                    table.AddCell(ls5)

                    c = c + 1
                Next
                '------------------------------------------------------------
                '   Agregar la tabla al archivo
                '------------------------------------------------------------
                pdfDoc.Add(table)
            Else
                pdfDoc.Add(New Chunk(vbNewLine))
                Dim pr = New Paragraph("Sin Tokens")
                pr.Alignment = Element.ALIGN_CENTER
                pdfDoc.Add(pr)
            End If

            '------------------------------------------------------------
            '   Cerrar el Documento
            '------------------------------------------------------------
            pdfDoc.Close()
            MsgBox("Reportes Generados en la carpeta del Archivo de Salida" + vbNewLine + "Ruta: " + ruta + "\" + nombreArchivo)


        Catch ex As Exception
            MsgBox("No se puede generar el reporte, porqué el archivo se encuentra abierto." + vbNewLine + "Cierrelo y analice nuevamente el archivo")
        End Try
    End Sub

    '------------------------------------------------------------
    '   REPORTE CON RECUPERACIÓN DE ERRORES
    '------------------------------------------------------------
    Private Sub generarReportesPDF(ruta As String, nombreArchivo As String, lista As ArrayList, lErrores As ArrayList, nReporte As String)


        Try
            '------------------------------------------------------------
            '   Configuraciones Iniciales
            '------------------------------------------------------------
            Dim pdfDoc As New Document(PageSize.A4, 30.0F, 30.0F, 30.0F, 30.0F)
            Dim pdfWrite As PdfWriter = PdfWriter.GetInstance(pdfDoc, New FileStream(ruta + "\" + nombreArchivo, FileMode.Create))

            '------------------------------------------------------------
            '   Se Abre el Documento
            '------------------------------------------------------------
            pdfDoc.Open()

            '------------------------------------------------------------
            '   Encabezado de la Universidad
            '------------------------------------------------------------
            Dim negrita As Font = FontFactory.GetFont("Verdana", 12, 1)
            pdfDoc.Add(New Paragraph("UNIVERSIDAD DE SAN CARLOS DE GUATEMALA", negrita))
            pdfDoc.Add(New Paragraph("FACULTAD DE INGENIERIA", negrita))
            pdfDoc.Add(New Paragraph("ESCUELA DE CIENCIAS", negrita))
            pdfDoc.Add(New Paragraph("INGENIERIA EN CIENCIAS Y SISTEMAS", negrita))
            pdfDoc.Add(New Paragraph("LENGUAJES FORMALES Y DE PROGRAMACION", negrita))

            '------------------------------------------------------------
            '   Imagen 
            '------------------------------------------------------------
            Dim urlLogo = Application.StartupPath + "\Resources\logo.png"
            If (System.IO.File.Exists(urlLogo)) Then
                'AGREGAMOS LA IMAGEN AL DOCUMENTO Y LA CONFIGURAMOS
                Dim logoUsac = iTextSharp.text.Image.GetInstance(urlLogo)
                logoUsac.ScaleToFit(140.0F, 120.0F)
                logoUsac.SetAbsolutePosition(450, 700)
                pdfDoc.Add(logoUsac)
            End If

            '------------------------------------------------------------
            '   titulo Reportes
            '------------------------------------------------------------
            pdfDoc.Add(New Chunk(vbNewLine))
            Dim fT As Font = FontFactory.GetFont("Arial", 18, 1)
            Dim titulo = New Paragraph("REPORTE " + nReporte.ToUpper, fT)
            titulo.Alignment = Element.ALIGN_CENTER
            pdfDoc.Add(titulo)

            '------------------------------------------------------------
            '   Encabezado de los archivos
            '------------------------------------------------------------

            Dim negrita2 As Font = FontFactory.GetFont("Verdana", 11.5, 1)
            pdfDoc.Add(New Chunk("Archivo Fuente: ", negrita2))
            pdfDoc.Add(New Chunk(lblNomArchivo.Text.ToString + vbNewLine))
            pdfDoc.Add(New Chunk("Archivo Salida: ", negrita2))
            pdfDoc.Add(New Chunk(nombreArchivoS.ToString.ToUpper))

            '------------------------------------------------------------
            '   Generación Tabla Tokens
            '------------------------------------------------------------
            Dim table As PdfPTable = New PdfPTable(5)
            Dim ancho() = {25, 285, 100, 35, 55}
            table.TotalWidth = 500.0F
            table.LockedWidth = True
            table.SetWidths(ancho)

            Dim fEnc As Font = FontFactory.GetFont("Verdana", 10, 1)
            fEnc.Color = iTextSharp.text.pdf.ExtendedColor.WHITE
            Dim c1 = New PdfPCell(New Phrase("No", fEnc))
            Dim c2 = New PdfPCell(New Phrase("Lexema", fEnc))
            Dim c3 = New PdfPCell(New Phrase("Tipo", fEnc))
            Dim c4 = New PdfPCell(New Phrase("Fila", fEnc))
            Dim c5 = New PdfPCell(New Phrase("Columna", fEnc))

            c1.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c2.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c3.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c4.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            c5.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK

            c1.HorizontalAlignment = 1
            c2.HorizontalAlignment = 1
            c3.HorizontalAlignment = 1
            c4.HorizontalAlignment = 1
            c5.HorizontalAlignment = 1

            c1.Padding = 5
            c2.Padding = 5
            c3.Padding = 5
            c4.Padding = 5
            c5.Padding = 5

            table.AddCell(c1)
            table.AddCell(c2)
            table.AddCell(c3)
            table.AddCell(c4)
            table.AddCell(c5)


            '------------------------------------------------------------
            '   Llenar la tabla con los datos
            '------------------------------------------------------------
            Dim c = 0
            Dim fDig As Font = FontFactory.GetFont("Verdana", 10)

            If lista.Count <> 0 Then
                For Each ls In lista
                    Dim ls1 = New PdfPCell(New Phrase(c, fDig))
                    Dim ls2 = New PdfPCell(New Phrase(ls.getLexema, fDig))
                    Dim ls3 = New PdfPCell(New Phrase(ls.getTipo.ToString.ToLower, fDig))
                    Dim ls4 = New PdfPCell(New Phrase(ls.getLinea, fDig))
                    Dim ls5 = New PdfPCell(New Phrase(ls.getColumna, fDig))

                    ls1.Padding = 3
                    ls2.Padding = 3
                    ls3.Padding = 3
                    ls4.Padding = 3
                    ls5.Padding = 3

                    ls1.FixedHeight = 5.0F

                    ls1.HorizontalAlignment = 1
                    ls4.HorizontalAlignment = 1
                    ls5.HorizontalAlignment = 1

                    table.AddCell(ls1)
                    table.AddCell(ls2)
                    table.AddCell(ls3)
                    table.AddCell(ls4)
                    table.AddCell(ls5)

                    c = c + 1
                Next
                '------------------------------------------------------------
                '   Agregar la tabla al archivo
                '------------------------------------------------------------
                pdfDoc.Add(table)
            Else
                Dim ls1 = New PdfPCell(New Phrase(" ", fDig))
                ls1.Padding = 3
                ls1.FixedHeight = 5.0F
                table.AddCell(ls1)
                table.AddCell("SIN TOKENS")
                table.AddCell(ls1)
                table.AddCell(ls1)
                table.AddCell(ls1)
                pdfDoc.Add(table)
            End If


            '------------------------------------------------------------
            '   Generación Tabla Errores
            '------------------------------------------------------------
            pdfDoc.NewPage()
            Dim tableErr As PdfPTable = New PdfPTable(1)
            tableErr.TotalWidth = 600.0F

            Dim e1 = New PdfPCell(New Phrase("Error", fEnc))
            e1.BackgroundColor = iTextSharp.text.pdf.ExtendedColor.BLACK
            e1.HorizontalAlignment = 1
            e1.Padding = 5
            tableErr.AddCell(e1)

            '------------------------------------------------------------
            '   Llenar la tabla con los Errores
            '------------------------------------------------------------

            If lErrores.Count <> 0 Then
                For Each ls In lErrores
                    tableErr.AddCell(ls.ToString)
                Next
                '------------------------------------------------------------
                '   Agregar la tabla al archivo
                '------------------------------------------------------------
                pdfDoc.Add(tableErr)
            Else
                tableErr.AddCell("SIN ERRORES")
                pdfDoc.Add(tableErr)
            End If


            '------------------------------------------------------------
            '   Cerrar el Documento
            '------------------------------------------------------------
            pdfDoc.Close()
            MsgBox("Reportes Generados en la carpeta del Archivo de Salida" + vbNewLine + "Ruta: " + ruta + "\" + nombreArchivo)


        Catch ex As Exception
            MsgBox("No se puede generar el reporte, porqué el archivo se encuentra abierto." + vbNewLine + "Cierrelo y analice nuevamente el archivo")
        End Try
    End Sub




    '------------------------------------------------------------
    '   MENU -> OPCION DE ANALIZAR 
    '------------------------------------------------------------
    Private Sub AnalizarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AnalizarToolStripMenuItem.Click
        btnAnalizar()
    End Sub


    '------------------------------------------------------------
    '   BOTON DE ANALIZAR 
    '------------------------------------------------------------
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        btnAnalizar()
    End Sub


    '------------------------------------------------------------
    '   METODO DEL ANALISIS Y GENERACION DE REPORTES DE TOKENS 
    '------------------------------------------------------------
    Private Sub btnAnalizar()
        listaTokensErroneos.Clear()
        listaTokensValidos.Clear()
        listaInstrucciones.Clear
        listaTexto.Clear()
        listaVariables.Clear()
        listaErrores.Clear()
        tabla.Rows.Clear()
        TextBox2.Clear()
        ProgressBar1.Value = 0
        nombreArchivoDefault = "salida.pdf"
        interlineado = interlineadoDefault
        tamanioLetra = tamanioLetraDefault
        direccionArchivo = direccionArchivoDefault
        nombreArchivoS = nombreArchivoDefault

        '------------------------------------------------------------
        '   Se realiza el analisis Lexico
        '------------------------------------------------------------
        Dim cadena2 = RichTextBox1.Lines
        For i = 0 To (cadena2.Length - 1)
            analizar(cadena2.GetValue(i).ToString.ToLower + " ", (i + 1))
        Next

        '------------------------------------------------------------
        '   ANALIZAR LOS BLOQUES
        '------------------------------------------------------------
        findInstructions(RichTextBox1.Text)
        findVariables(RichTextBox1.Text)
        findText(RichTextBox1.Text)


        '------------------------------------------------------------
        '   GENERAR ARCHIVO DE SALIDA
        '------------------------------------------------------------
        generate()
        Dim analizadorSintacto As AnalizadorSintactico = New AnalizadorSintactico
        analizadorSintacto.parsear(listaTokensValidos)

        '------------------------------------------------------------
        '   VER LOS TOKENS VALIDOS
        '------------------------------------------------------------
        For Each x As Token In listaTokensValidos
            Dim s() As String = {x.getTipo, x.getlexema, x.getLinea, x.getcolumna}
            tabla.Rows.Add(s)
        Next

        '------------------------------------------------------------
        '   VER LOS TOKENS ERRONES
        '------------------------------------------------------------
        For Each x As Token In listaTokensErroneos
            Dim s() As String = {"Error: " + x.getTipo, x.getlexema, x.getLinea, x.getcolumna}
            tabla.Rows.Add(s)
        Next


        '------------------------------------------------------------
        '   GENERAR REPORTES VALIDOS/ERRONEOS
        '------------------------------------------------------------
        If (nombreArchivoS.ToLower.Contains(".pdf")) Then
            generarReportesPDF(direccionArchivo, "TokensValidos_" + nombreArchivoS, listaTokensValidos, "tokens validos")
            generarReportesPDF(direccionArchivo, "TokensErroneos_" + nombreArchivoS, listaTokensErroneos, listaErrores, "tokens erroneos")
        Else
            generarReportesPDF(direccionArchivo, "TokensValidos_" + nombreArchivoS + ".pdf", listaTokensValidos, "tokens validos")
            generarReportesPDF(direccionArchivo, "TokensErroneos_" + nombreArchivoS + ".pdf", listaTokensErroneos, listaErrores, "tokens erroneos")
        End If
        Process.Start("explorer.exe", direccionArchivo)
    End Sub

    '------------------------------------------------------------
    '   MENU -> BOTON SALIR 
    '------------------------------------------------------------
    Private Sub SalirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SalirToolStripMenuItem.Click
        Application.Exit()
    End Sub

    '------------------------------------------------------------
    '   BLOQUE INSTRUCCIONES
    '------------------------------------------------------------
    Private Sub findInstructions(txt As String)

        Dim s = txt.ToLower.Split("}")

        For Each ss In s
            If (ss.Contains("instrucciones")) Then
                Dim tx = ss.Split("{")
                For Each t In tx
                    Dim funciones = t.Trim.Split(";")
                    For Each f In funciones
                        If (f.Contains("interlineado") Or f.Contains("tamanio_letra") Or f.Contains("nombre_archivo") Or f.Contains("direccion_archivo")) Then
                            listaInstrucciones.Add(f.Trim)
                        End If
                    Next
                Next
            End If
        Next

        'For Each l In listaInstrucciones
        '    MsgBox(l.ToString + vbNewLine)
        'Next
    End Sub

    '------------------------------------------------------------
    '   BLOQUE TEXTOS
    '------------------------------------------------------------
    Private Sub findText(txt As String)

        Dim s = txt.Replace(vbCrLf, "").Replace(vbLf, "").Replace("*/", "*/;").ToLower.Split("}")

        For Each ss In s
            If (ss.Contains("texto{")) Then
                Dim tx = ss.Split("{")
                tx(0) = ""
                For Each t In tx
                    Dim funciones = t.Split(";")
                    For Each f In funciones
                        If (f.Equals(" ") Or f.Equals("")) Then
                        Else
                            listaTexto.add(f.Trim)
                            listaTexto.remove(vbNewLine)
                            listaTexto.remove(vbCrLf)
                            listaTexto.remove(vbLf)
                            listaTexto.remove("")
                            listaTexto.remove(" ")
                        End If
                    Next
                Next
            End If
        Next

        'For Each l In listaTexto
        '    MsgBox(l.ToString + vbNewLine)
        'Next
    End Sub

    Private Sub AcercaDeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AcercaDeToolStripMenuItem.Click
        Me.Hide()
        AcercaDe.Show()
    End Sub

    Private Sub AbrirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AbrirToolStripMenuItem.Click
        Try
            OpenFileDialog1.Filter = "ack Files (*.ack) |*.ack"
            If (OpenFileDialog1.ShowDialog() = DialogResult.OK) Then
                If (Path.GetExtension(OpenFileDialog1.FileName.ToLower).Equals(".ack")) Then
                    'nomArchivoP.Text = System.IO.Path.GetFileName(OpenFileDialog1.FileName)
                    RichTextBox1.Clear()
                    Dim ruta As String = OpenFileDialog1.FileName
                    Dim leer As New StreamReader(ruta, System.Text.Encoding.UTF8)
                    lblNomArchivo.Text = System.IO.Path.GetFileName(OpenFileDialog1.FileName)
                    Dim l As String = leer.ReadToEnd
                    RichTextBox1.AppendText(l.ToLower)

                    leer.Close()
                Else
                    MsgBox("Debes cargar un archivo con extension .ack")
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub RichTextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles RichTextBox1.KeyPress
        If (lblNomArchivo.Text = "") Then
            lblNomArchivo.Text = "Archivo Nuevo - * "
        Else
            If lblNomArchivo.Text.Contains(" - *") Then
                Dim s = lblNomArchivo.Text
                lblNomArchivo.Text = s
            Else
                Dim s = lblNomArchivo.Text
                lblNomArchivo.Text = s + " - *"
            End If
        End If
    End Sub

    Private Sub GuardarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuardarToolStripMenuItem.Click
        Try
            SaveFileDialog1.Filter = "ack Files (*.ack) |*.ack"

            If (OpenFileDialog1.FileName = "OpenFileDialog1") Then
                SaveFileDialog1.ShowDialog()
                Dim archivo As StreamWriter = File.CreateText(SaveFileDialog1.FileName)
                If IsNothing(RichTextBox1.Text) = False Then
                    archivo.WriteLine(RichTextBox1.Text)
                    archivo.Flush()
                    archivo.Close()
                    lblNomArchivo.Text = System.IO.Path.GetFileName(SaveFileDialog1.FileName)
                End If
                MsgBox("Archivo Guardado")
            Else
                Dim archivo As StreamWriter = File.CreateText(OpenFileDialog1.FileName)
                archivo.WriteLine(RichTextBox1.Text)
                archivo.Flush()
                archivo.Close()
                lblNomArchivo.Text = System.IO.Path.GetFileName(OpenFileDialog1.FileName)
                MsgBox("Archivo Guardado")
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub NuevoArchivoEntradaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NuevoArchivoEntradaToolStripMenuItem.Click
        OpenFileDialog1.FileName = "OpenFileDialog1"
        SaveFileDialog1.FileName = "SaveFileDialog1"
        lblNomArchivo.Text = ""
        tabla.Rows.Clear()
        RichTextBox1.Clear()
    End Sub

    Private Sub GuardarComoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuardarComoToolStripMenuItem.Click
        Try
            SaveFileDialog1.Filter = "ack Files (*.ack) |*.ack"

            If (OpenFileDialog1.FileName = "OpenFileDialog1") Then
                SaveFileDialog1.ShowDialog()
                Dim archivo As StreamWriter = File.CreateText(SaveFileDialog1.FileName)
                If IsNothing(RichTextBox1.Text) = False Then
                    archivo.WriteLine(RichTextBox1.Text)
                    archivo.Flush()
                    archivo.Close()
                    lblNomArchivo.Text = System.IO.Path.GetFileName(SaveFileDialog1.FileName)
                End If
                MsgBox("Archivo Guardado")
            Else
                SaveFileDialog1.ShowDialog()
                Dim archivo As StreamWriter = File.CreateText(SaveFileDialog1.FileName)
                archivo.WriteLine(RichTextBox1.Text)
                archivo.Flush()
                archivo.Close()
                lblNomArchivo.Text = System.IO.Path.GetFileName(SaveFileDialog1.FileName)
                MsgBox("Archivo Guardado")
            End If
        Catch ex As Exception
        End Try
    End Sub

    '-----------------------------------------------------------
    '   COLOREAR LOS LEXEMAS CON CHECKBOX'S
    '-----------------------------------------------------------
    'PALABRAS RESERVADAS
    Private Sub colorearPalabrasReservadas(color As Color)
        For Each pr In palabrasReservadas
            For i = 0 To (RichTextBox1.Text.LastIndexOf(pr))
                RichTextBox1.Find(pr, i, RichTextBox1.TextLength, RichTextBoxFinds.MatchCase)
                RichTextBox1.SelectionColor = color
            Next
        Next
    End Sub

    'NUMEROS
    Private Sub colorearNumeros(color As Color)
        For Each pr In listaTokensValidos
            Dim tkn As Token = pr
            'MsgBox("Token: " + tkn.getlexema.ToString + " Tipo: " + tkn.getTipo.ToString)
            If (tkn.getTipo = Tipo.NUMERO_ENTERO) Then
                For i = 0 To (RichTextBox1.Text.LastIndexOf(tkn.getlexema.ToString))
                    RichTextBox1.SelectionBackColor = color
                    RichTextBox1.Find(tkn.getlexema.ToString, i, RichTextBox1.TextLength, RichTextBoxFinds.MatchCase)
                Next
            ElseIf (tkn.getTipo = Tipo.NUMERO_DECIMAL) Then
                For i = 0 To (RichTextBox1.Text.LastIndexOf(tkn.getlexema.ToString))
                    RichTextBox1.SelectionBackColor = color
                    RichTextBox1.Find(tkn.getlexema.ToString, i, RichTextBox1.TextLength, RichTextBoxFinds.MatchCase)
                Next
            End If
        Next
    End Sub

    'CADENAS DE TEXTO
    Private Sub colorearCadenasDeTexto(color As Color)
        For Each pr In listaTokensValidos
            Dim tkn As Token = pr
            'MsgBox("Token: " + tkn.getlexema.ToString + " Tipo: " + tkn.getTipo.ToString)
            If (tkn.getTipo = Tipo.CADENA_DE_TEXTO) Then
                For i = 0 To (RichTextBox1.Text.LastIndexOf(tkn.getlexema.ToString))
                    RichTextBox1.Find(tkn.getlexema.ToString, i, RichTextBox1.TextLength, RichTextBoxFinds.MatchCase)
                    RichTextBox1.SelectionColor = color
                Next
            End If
        Next
    End Sub

    'COMENTARIOS
    Private Sub colorearComentarios(color As Color)
        For Each pr In listaComentarios
            For i = 0 To (RichTextBox1.Text.LastIndexOf(pr.ToString))
                RichTextBox1.Find(pr.ToString, i, RichTextBox1.TextLength, RichTextBoxFinds.MatchCase)
                RichTextBox1.SelectionBackColor = color
            Next
        Next
    End Sub

    'Simbolos
    Private Sub colorearSimbolos(color As Color)
        For Each pr In listaSimbolos
            For i = 0 To (RichTextBox1.Text.LastIndexOf(Chr(pr.ToString)))
                RichTextBox1.Find(Chr(pr.ToString), i, RichTextBox1.TextLength, RichTextBoxFinds.MatchCase)
                RichTextBox1.SelectionColor = color
            Next
        Next
    End Sub


    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If (CheckBox1.Checked) Then
            If (listaTokensValidos.Count <> 0) Then
                'PALABRAS RESERVADAS
                colorearPalabrasReservadas(Color.Blue)
            Else
                MsgBox("No se encontraron lexemas")
                CheckBox1.Checked = False
            End If
        Else
            colorearPalabrasReservadas(Color.Black)
            CheckBox1.Checked = False
        End If
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If (CheckBox2.Checked) Then
            If (listaTokensValidos.Count <> 0) Then
                colorearNumeros(Color.Yellow)
            Else
                MsgBox("No se encontraron lexemas")
                CheckBox2.Checked = False
            End If
        Else
            colorearNumeros(Color.Transparent)
            CheckBox2.Checked = False
        End If
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        If (CheckBox3.Checked) Then
            If (listaTokensValidos.Count <> 0) Then
                colorearCadenasDeTexto(Color.ForestGreen)
            Else
                MsgBox("No se encontraron lexemas")
                CheckBox3.Checked = False
            End If
        Else
            colorearCadenasDeTexto(Color.Black)
            CheckBox3.Checked = False
        End If
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        If (CheckBox4.Checked) Then
            If (listaTokensValidos.Count <> 0) Then
                colorearComentarios(Color.Gray)
            Else
                MsgBox("No se encontraron lexemas")
                CheckBox4.Checked = False
            End If
        Else
            colorearComentarios(Color.Transparent)
            CheckBox4.Checked = False
        End If
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        If (CheckBox5.Checked) Then
            If (listaTokensValidos.Count <> 0) Then
                colorearSimbolos(Color.Orange)
            Else
                MsgBox("No se encontraron lexemas")
                CheckBox5.Checked = False
            End If
        Else
            colorearSimbolos(Color.Black)
            CheckBox5.Checked = False
        End If
    End Sub

    '------------------------------------------------------------
    '   BLOQUE VARIABLES
    '------------------------------------------------------------
    Private Sub findVariables(txt As String)
        Dim qs = txt.Replace(vbCrLf, " ").Replace(vbLf, "")
        Dim s = qs.ToLower.Split("}")

        Try
            For Each ss In s
                If (ss.Contains("variables{") Or ss.Contains("variables {")) Then
                    Dim tx = ss.Trim.Split("{")
                    For Each t In tx
                        Dim funciones = t.Replace("variables", "").Trim.Split(";")
                        For Each f In funciones
                            If (f.Equals("") Or f.Equals(" ")) Then
                            Else
                                Dim vars = f.Trim.Split(":")
                                Dim nVars = vars(0).Trim().Split(",")
                                Dim asig = vars(1).Split("=")
                                Dim tipo = asig(0)
                                Dim valor
                                Try
                                    valor = asig(1)
                                Catch ex As Exception
                                    valor = "0"
                                End Try
                                For Each v In nVars
                                    If (listaVariables.contains(v.ToString)) Then
                                    Else
                                        Dim existe = False
                                        For Each e In listaVariables
                                            If (e.getNombre.Equals(v.ToString)) Then
                                                existe = True
                                            End If
                                        Next

                                        If (existe = False) Then
                                            listaVariables.add(New Variable(v.ToString, valor.ToString.Replace(Chr(34), ""), tipo.ToString))
                                        Else
                                            listaErrores.Add("No puede crear la variable " + v.ToString + " porque ya esta definidia")
                                        End If

                                    End If
                                Next
                            End If
                        Next
                    Next
                End If
            Next
        Catch ex As Exception

        End Try
        'For Each l In listaVariables
        '    MsgBox("Variable| " + l.getNombre + " |Valor| " + l.getValor + " |Tipo| " + l.getTipo)
        'Next
    End Sub


    '------------------------------------------------------------
    '   GENERAR ARCHIVO 
    '------------------------------------------------------------
    Private Sub generate()
        My.Computer.FileSystem.CreateDirectory("C:\pdf")

        Try
            For Each ls In listaInstrucciones
                Dim tx = ls.ToString.Replace(Chr(34), "").Replace(")", "").Split("(")

                If (tx(0).Contains("nombre_archivo")) Then
                    nombreArchivoS = tx(1)
                ElseIf (tx(0).Contains("direccion_archivo")) Then
                    direccionArchivo = tx(1)
                    If (Directory.Exists(direccionArchivo)) Then
                    Else
                        direccionArchivo = direccionArchivoDefault
                        MsgBox("La ruta especificada no existe." + vbNewLine + "El archivo se guardara en C:\pdf\")
                    End If
                ElseIf (tx(0).Contains("interlineado")) Then
                    interlineado = CInt(tx(1))
                ElseIf (tx(0).Contains("tamanio_letra")) Then
                    tamanioLetra = CInt(tx(1))
                End If
            Next
        Catch ex As Exception
        End Try

        '------------------------------------------------------------
        '   Configuraciones Iniciales
        '------------------------------------------------------------
        Dim pdfDoc As New Document(PageSize.A4, 50.0F, 50.0F, 50.0F, 50.0F)

        Try
            If (Path.GetExtension(nombreArchivoS).Equals(".pdf")) Then
                Dim pdfWrite As PdfWriter = PdfWriter.GetInstance(pdfDoc, New FileStream(direccionArchivo + "\" + nombreArchivoS, FileMode.Create))
                MsgBox("Archivo de Salida generado Exitosamente")
            Else
                MsgBox("No se pudo generar el archivo de salida, verifique que la extension sea .pdf")
            End If
        Catch ex As Exception
            MsgBox("El archivo se encuentra en ejecución, cierrelo")
        End Try

        Dim pdfFont As Font = FontFactory.GetFont("Arial", tamanioLetra)
        Dim pdfFontBold As Font = FontFactory.GetFont("Arial", tamanioLetra, 1)

        '------------------------------------------------------------
        '   Se Abre el Documento
        '------------------------------------------------------------
        pdfDoc.Open()

        '------------------------------------------------------------
        '   Generar el texto
        '------------------------------------------------------------
        For Each i In listaTexto

            '------------------------------------------------------------
            '   Comentarios
            '------------------------------------------------------------
            If (i.ToString.Contains("/*")) Then
                listaComentarios.add(i.ToString.Trim)
                '------------------------------------------------------------
                '   Salto De Linea
                '------------------------------------------------------------
            ElseIf (i.ToString.Replace(Chr(34), "").Trim.Contains("linea_en_blanco")) Then
                pdfDoc.Add(New Paragraph("" + vbNewLine + vbNewLine))
                '------------------------------------------------------------
                '   Negrita
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("[+")) Then
                Dim r = i.ToString.Trim.Replace("[", "").Replace(Chr(34), "").Split("+")
                pdfDoc.Add(New Chunk(r(0) + " ", pdfFont))
                pdfDoc.Add(New Chunk(r(1) + " ", pdfFontBold))
                '------------------------------------------------------------
                '   Subrayado
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("[*")) Then
                Dim r = i.ToString.Trim.Replace("[", "").Replace(Chr(34), "").Split("*")

                pdfDoc.Add(New Chunk(r(0) + "  ", pdfFont))

                Dim n = New Chunk(r(1) + "  ", pdfFont)
                n.SetUnderline(1.5F, 3.5F)
                pdfDoc.Add(n)
                '------------------------------------------------------------
                '   Imagen
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("imagen(")) Then
                Dim r = i.ToString.Replace("imagen(", "imagen(,").Replace(")", "").Replace(Chr(34), "").Split(",")

                Dim ruta, tamX, tamY
                Try
                    pdfDoc.Add(New Chunk(r(0).ToString.Trim.Replace("imagen(", "") + vbNewLine))
                    ruta = r(1)
                    tamX = r(2)
                    tamY = r(3)
                Catch ex As Exception
                    ruta = r(0)
                    tamX = r(1)
                    tamY = r(2)
                End Try
                'MsgBox(ruta.ToString + " " + tamX.ToString + " " + tamY.ToString)

                If (System.IO.File.Exists(ruta)) Then
                    'AGREGAMOS LA IMAGEN AL DOCUMENTO Y LA CONFIGURAMOS
                    Dim img = iTextSharp.text.Image.GetInstance(ruta)
                    img.ScaleToFit(tamX + 100, tamY + 100)
                    img.Alignment = iTextSharp.text.Image.ALIGN_CENTER
                    pdfDoc.Add(img)
                End If
                '------------------------------------------------------------
                '   Funcion de Numeros
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("numeros(")) Then
                pdfDoc.Add(New Chunk("" + vbNewLine))
                Dim n = i.ToString.Replace("numeros(", "numeros,").Replace(")", ",").Replace(Chr(34), "").Split(",")
                Dim lista As New List(True, n.Length)
                For jn = 0 To (n.Length - 1)
                    If (jn = 0) Then
                        pdfDoc.Add(New Chunk(n(0).Replace("numeros", "") + "  ", pdfFont))
                        Continue For
                    ElseIf (n(jn).ToString <> "") Then
                        lista.Add(New ListItem("   " + n(jn), pdfFont))
                    End If
                Next
                pdfDoc.Add(lista)
                '------------------------------------------------------------
                '   Funcion VAR que muestra el valor de la variable
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("var[")) Then
                Dim v = i.ToString.Replace("]", "").Replace("var[", "var|[").Replace(Chr(34), "").Split("[")
                Dim t = v(0).ToString.Replace("var|", "")

                pdfDoc.Add(New Chunk(t, pdfFont))
                For Each l In listaVariables
                    If (l.getNombre.ToString.Trim.Equals(v(1))) Then
                        pdfDoc.Add(New Chunk(l.getValor.ToString, pdfFont))
                    End If
                Next
                '------------------------------------------------------------
                '   Funcion SUMA que suma los valores dados
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("suma(")) Then
                Dim v = i.ToString.Replace(")", "").Replace("suma(", "suma(,".Replace(Chr(34), "")).Trim.Split(",")
                Dim suma = 0

                For Each vv In v
                    Dim res = Int32.TryParse(vv, 0)
                    If (res) Then
                        suma += CInt(vv)
                    End If
                Next

                For Each l In listaVariables
                    For Each vv In v
                        If (l.getNombre.ToString.Trim.Equals(vv)) Then
                            Try
                                suma += CInt(l.getValor)
                            Catch ex As Exception
                                listaErrores.add("No puedes usar la función suma con una cadena de texto")
                            End Try
                        End If
                    Next
                Next
                pdfDoc.Add(New Chunk(v(0).Replace("suma(", "").Replace(Chr(34), ""), pdfFont))
                pdfDoc.Add(New Chunk(suma.ToString + vbNewLine, pdfFont))
                '------------------------------------------------------------
                '   Funcion RESTA que resta los valores dados
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("resta(")) Then
                Dim v = i.ToString.Replace(")", "").Replace("resta(", "resta(,").Replace(Chr(34), "").Trim.Split(",")
                Dim resta = 0
                Dim cont = 0

                For Each vv In v
                    Dim res = Int32.TryParse(vv, 0)
                    If (res) Then
                        resta = resta - CLng(vv)
                    Else
                        For Each l In listaVariables
                            If (l.getNombre.ToString.Trim.Equals(vv.ToString.Trim)) Then
                                If (l.getTipo.ToString.Trim.Equals("entero")) Then
                                    If (cont = 0) Then
                                        resta = CLng(l.getValor)
                                    Else
                                        resta = resta - CLng(l.getValor)
                                    End If
                                    cont += 1
                                Else
                                    If Not listaErrores.contains("No puedes usar la función resta con una cadena de texto") Then
                                        listaErrores.add("No puedes usar la función resta con una cadena de texto")
                                    End If
                                End If
                            Else
                                If Not listaErrores.contains("No puedes usar la función resta con una cadena de texto") Then
                                    listaErrores.add("No puedes usar la función resta con una cadena de texto")
                                End If
                            End If
                        Next
                    End If
                Next
                pdfDoc.Add(New Chunk(v(0).Replace("resta(", ""), pdfFont))
                pdfDoc.Add(New Chunk(resta.ToString + vbNewLine, pdfFont))
                '------------------------------------------------------------
                '   Funcion MULTIPLICACIÓN que suma los valores dados
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("multiplicacion(")) Then
                Dim v = i.ToString.Replace(")", "").Replace("multiplicacion(", "multiplicacion(,").Replace(Chr(34), "").Trim.Split(",")
                Dim times = 1

                For Each vv In v
                    Dim res = Int32.TryParse(vv, 0)
                    If (res) Then
                        times *= CInt(vv)
                    End If
                Next

                For Each l In listaVariables
                    For Each vv In v
                        If (l.getNombre.ToString.Trim.Equals(vv)) Then
                            Try
                                times *= CInt(l.getValor)
                            Catch ex As Exception
                                If Not listaErrores.contains("No puedes usar la función multiplicación con una cadena de texto") Then
                                    listaErrores.add("No puedes usar la función multiplicación con una cadena de texto")
                                End If
                            End Try
                        Else
                            If Not listaErrores.contains("No puedes usar la función multiplicación con una cadena de texto") Then
                                listaErrores.add("No puedes usar la función multiplicación con una cadena de texto")
                            End If
                        End If
                    Next
                Next
                pdfDoc.Add(New Chunk(v(0).Replace("multiplicacion(", ""), pdfFont))
                pdfDoc.Add(New Chunk(times.ToString + vbNewLine, pdfFont))
                '------------------------------------------------------------
                '   Funcion DIVISION que divide los valores dados
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("division(")) Then
                Dim v = i.ToString.Replace(")", "").Replace("division(", "division(,").Replace(Chr(34), "").Trim.Split(",")
                Dim division As Double = 1.0
                Dim cont = 0

                For Each vv In v
                    Dim res = Int32.TryParse(vv, 0)
                    If (res) Then
                        division = division / CLng(vv)
                    Else
                        For Each l In listaVariables
                            If (l.getNombre.ToString.Trim.Equals(vv.ToString.Trim)) Then
                                If (l.getTipo.ToString.Trim.Equals("entero")) Then
                                    If (cont = 0) Then
                                        division = CLng(l.getValor)
                                    Else
                                        division = division / CLng(l.getValor)
                                    End If
                                    cont += 1
                                Else
                                    If Not listaErrores.contains("No puedes usar la función division con una variable de texto") Then
                                        listaErrores.add("No puedes usar la función division con una variable de texto")
                                    End If
                                End If
                            Else
                                If Not listaErrores.contains("No puedes usar la función division con una cadena de texto") Then
                                    listaErrores.add("No puedes usar la función division con una cadena de texto")
                                End If
                            End If
                        Next
                    End If
                Next
                pdfDoc.Add(New Chunk(v(0).Replace("division(", ""), pdfFont))
                pdfDoc.Add(New Chunk(division.ToString + vbNewLine))

                '------------------------------------------------------------
                '   Funcion PROMEDIO 
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("promedio(")) Then
                Dim v = i.ToString.Replace(")", "").Replace("promedio(", "promedio(,").Replace(Chr(34), "").Trim.Split(",")
                Dim suma = 0
                Dim n = 0
                Dim promedio = 0

                For Each vv In v
                    Dim res = Int32.TryParse(vv, 0)
                    If (res) Then
                        'MsgBox("suma = " + suma.ToString)
                        suma += CInt(vv)
                        'MsgBox("suma = " + suma.ToString)
                        n += 1
                    End If
                Next

                For Each l In listaVariables
                    For Each vv In v
                        If (l.getNombre.ToString.Trim.Equals(vv)) Then
                            'MsgBox("suma = " + suma.ToString)
                            suma += CInt(l.getValor)
                            'MsgBox("suma = " + suma.ToString)
                            n += 1
                        Else
                            If Not listaErrores.contains("No puedes usar la función promedio con una cadena de texto") Then
                                listaErrores.add("No puedes usar la función promedio con una cadena de texto")
                            End If
                        End If
                    Next
                Next
                promedio = (suma / n)
                pdfDoc.Add(New Chunk(v(0).Replace("promedio(", ""), pdfFont))
                pdfDoc.Add(New Chunk(promedio.ToString, pdfFont))
                '------------------------------------------------------------
                '   Funcion ASIGNAR 
                '------------------------------------------------------------
            ElseIf (i.ToString.Contains("asignar(")) Then
                Dim v = i.ToString.Replace(")", "").Replace("asignar(", "asignar(,").Replace(Chr(34), "").Trim.Split(",")
                Dim nombreVar = v(1).ToString.Trim
                Dim valorVar = v(2).ToString.Trim

                If (v.Count() > 3) Then
                    For z = 3 To (v.Count - 1)
                        valorVar = valorVar + ", " + v(z).ToString
                        MsgBox(valorVar)
                    Next
                End If

                For Each var In listaVariables
                    If (var.getNombre.ToString.Trim.Equals(nombreVar)) Then
                        If (var.getTipo.ToString.Trim.Equals("entero")) Then
                            Dim verificarValor = False
                            Dim isNumeric = Double.TryParse(valorVar, verificarValor)

                            If (verificarValor) Then
                                var.setValor(valorVar)
                            Else
                                Dim size = listaVariables.count()
                                Dim cont = 0

                                For Each verificarVar In listaVariables
                                    cont = cont + 1

                                    If (verificarVar.getNombre.ToString.Trim.Equals(valorVar)) Then
                                        If (verificarVar.getTipo.ToString.Trim.Equals("entero")) Then
                                            var.setValor(verificarVar.getValor.ToString)
                                        Else
                                            'listaErrores.Add("Asignación en la variable: " + v(1).ToString + " se esperara un valor númerico")
                                            If Not listaErrores.contains("Asignación en la variable: " + v(1).ToString + " se esperara un valor númerico") Then
                                                listaErrores.add("Asignación en la variable: " + v(1).ToString + " se esperara un valor númerico")
                                            End If
                                        End If
                                        Exit For
                                    Else
                                        If Not listaErrores.contains("No puedes usar la variable " + valorVar + " porque no existe") Then
                                            listaErrores.add("No puedes usar la variable " + valorVar + " porque no existe")
                                        End If
                                    End If
                                Next
                            End If
                        ElseIf (var.getTipo.ToString.Trim.Equals("cadena")) Then

                            For Each variableLista In listaVariables
                                Dim variableAux As Variable = variableLista
                                If (variableAux.getNombre.ToLower.Trim.Equals(valorVar)) Then
                                    var.setValor(variableAux.getValor)
                                    Exit For
                                Else
                                    var.setValor(valorVar)
                                End If
                            Next
                        End If
                        Exit For
                    Else
                        If Not listaErrores.contains("No puedes usar la variable " + nombreVar + " porque no existe") Then
                            listaErrores.add("No puedes usar la variable " + nombreVar + " porque no existe")
                        End If
                    End If
                Next

            Else
                Dim pr = New Paragraph(i.ToString.Trim.Replace(Chr(34), ""), pdfFont)
                pr.SetLeading(0.5, interlineado)
                pdfDoc.Add(pr)
            End If
        Next

        '------------------------------------------------------------
        '   Cerrar el Documento
        '------------------------------------------------------------
        Try
            pdfDoc.Close()
        Catch ex As Exception

        End Try



    End Sub


End Class





'AGREGAR PARRAFOS
'For Each x As Token In listaTokensValidos
'pdfDoc.Add(New Paragraph(x.getTipo.ToString + "   -----------   " + x.getlexema.ToString))
'Next

'AGREGAR LISTAS
'Dim lista As New List(True, 3)
'lista.Add(New ListItem("  Elemento 1"))
'lista.Add(New ListItem("  Elemento 2"))
'lista.Add(New ListItem("  Elemento 3"))
'pdfDoc.Add(lista)
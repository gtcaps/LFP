INSTRUCCIONES
{
	nombre_archivo("henry.pdf");
	interlineado(2.5);
	tamanio_letra(12);
}

VARIABLES
{
	x1,x2,x3: entero=20; Y1,Y2: cadena="texto";
	Nombre: cadena="Pedro Pomez";
	x10: entero;
	y3: cadena;
}

TEXTO
{
	[+Titulo Principal+];
	linea_en_blanco;
	Linea_en_blanco;
	"Este es el primer párrafo que contendrá el documento, notese que pueden"
	[+crearse varios+];
	". Los documentos generados pueden tener en el mismo texto"
	Linea_en_blanco;
	Linea_en_blanco; "varios saltos de línea intercalados"}
	
TEXTO{
	[*A su vez... usar las funciones del sistema*];
	Numeros("suma","promedio","etc");
	"A demás debe mostrar los valores de variables como x1 = " var[x1];
	Linea_en_blanco;
	linea_en_blanco; linea_en_blanco;
	Asignar(x10,100);
	"El calculo de sumas x2 + x10 = " suma(x2,x10); Linea_en_blanco;
	"El ingreso de imagenes como la siguiente:"
	Imagen("C:\Users\aybso\Downloads\f1.jpg",30,30);
	Imagen("C:\Users\aybso\Downloads\f2.jpg",30,30);
	Imagen("C:\Users\aybso\Downloads\logo.bmp",100,100);
}

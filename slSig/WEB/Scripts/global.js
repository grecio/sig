function AtivarPesquisa(strTab, strDest, strFltExt, strOrdExt, strPath) {

	var strHREF = '';
	var oWnd;
	
	if (strPath) {strHREF += strPath};

	strHREF += '_pesquisa.aspx?tab=' + strTab + ''
	
	if (strDest) {strHREF += '&dest=' + strDest};
	if (strFltExt) {strHREF += '&fltext=' + strFltExt};
	if (strOrdExt) {strHREF += '&ordext=' + strOrdExt};

	oWnd = window.open(strHREF, 'pesquisa', 'top=20,left=20,width=760,height=540,menubar=no,statusbar=yes,resizable=yes');
	oWnd.focus();

}

//--------------------------------------------------------------------------------------------------

function docLocation(strHREF){
	
	var dtAgora = new Date;
	
	document.location = strHREF + '&dt=' + dtAgora.toString();
	
}

//------------------------------------------------------------------------------------------------------
function apenasNumDec(Obj, casas, tam){
	 var Numero = Obj.value;
	 var NumeroInt, NumeroDec;
	 	 
	 //Limita o tamanho de caracteres do número.
	 if(Numero.length >= tam){	 	
	 	event.keycode = 0;
	  	return(false);
	 }
	 	 
	 //Só permite a digitação dessas teclas.
	 if(event.keyCode < 48 || event.keyCode > 57 || event.keyCode == 8) {
		event.keyCode = 0;
	 	 return(false);   
	 } 
	 
	 //Retira a vírgula.
	 var rVg      
	 rVg = Numero.replace(",", "");
	 Numero = rVg;
	 
	 //Retira o ponto.
	 var NumeroSemPonto = Numero.split(".");
	 if(NumeroSemPonto[0] != ""){
	  	Numero = "";
	  	for(i = 0; i <= NumeroSemPonto.length - 1; i++)
	   		Numero = Numero + NumeroSemPonto[i];
	 	}
	 
	 //Caso o número não contenha parte inteira são acrescentados
	 //zeros à esquerda.
	 if(Numero.length < casas){
	 	var zeros = "";
		for(i = 0; i < casas - Numero.length; i++){
			zeros += "0";
		}
		Numero = zeros + Numero;	
	 }
	  
	  //Se o número tiver parte inteira e o primeiro número for '0'
	  //o mesmo é desprezado.
	  if((Numero.substring(0, 1) == "0") && (Numero.length > casas))
	  		NumeroInt = Numero.substring(1, Numero.length - (casas - 1));
	  else
	  //Caso possua parte inteira e o primeiro não for zero,
	  //o primeiro número é considerado.
	   		NumeroInt = Numero.substring(0, Numero.length - (casas - 1));

	 //Determina a parte decimal.
	 NumeroDec = Numero.substring(Numero.length - (casas - 1), Numero.length);
	
	 //Coloca o ponto na parte inteira.
	 if(NumeroInt.length > 1){
	  	var NumeroIntAux = "";
	  	var vetorNum = new Array(NumeroInt.length - 1);
	    for(i = 0; i < NumeroInt.length; i++){
			vetorNum[i] = NumeroInt.substring(i, i + 1);
	  	}
	  var j = 0;
	  for(i = NumeroInt.length - 1; i > -1; i--){
	  	if(j == 3){
			j = 1;
			NumeroIntAux = vetorNum[i] + "." + NumeroIntAux; 
	   	}
	   	else{
			NumeroIntAux = vetorNum[i] + NumeroIntAux 
			j++;
	   }
	  }
	  NumeroInt = NumeroIntAux;
 }
 
	 //Concatena a parte inteira com a parte decimal.
	 Numero = NumeroInt + "," + NumeroDec;
	
	 //Atribui o número ao objeto.
	 Obj.value = Numero;
}

// ------------------------------------------------------------------------------------------------------

function FormatarNumero(dNumero, iDecimais) {
   var sAux = "";
   var bTirouPonto = false;
	 var bTratouSinal = false;

   for (var i = dNumero.length - 1; i > -1; i--) {
      var letra = dNumero.substring (i,i+1);
      if (letra == '.' || letra == ',') {
         if (!bTirouPonto) {
            sAux = "." + sAux;
            bTirouPonto = true;
         }
      }
      else if (letra == '-') {
         if (!bTratouSinal) {
            sAux = "-" + sAux;
            bTratouSinal = true;
         }
			}
      else if (! isNaN(letra)) {
        sAux = letra + sAux;
      }
   }
   if (isNaN(parseInt(sAux)) || sAux == '.')
      return (0);
   else
	 		if (iDecimais) sAux = parseFloat(sAux).toFixed(iDecimais);

	 return (sAux);

}

//--------------------------------------------------------------------------------------------------

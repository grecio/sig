function verificarMascara(tecla,m) {
		
	if (m == '?') {
		return ('0123456789ABCDEFGHIJKLMNOPQRSTUVXWYZ'.toLowerCase().indexOf(tecla.toLowerCase()) > -1);
	} else if (m == '#') {
		return ('0123456789'.indexOf(tecla) > -1);
	} else if (m == '$') {
		return ('ABCDEFGHIJKLMNOPQRSTUVXWYZ'.toLowerCase().indexOf(tecla.toLowerCase()) > -1);
	} else return false;
}

//-------------------------------------------------------------------------------------------------------------

function mascaraCampo(elemento, mascara, event){
	if (navigator.appName.toLowerCase().indexOf("netscape") > -1)
		ntecla=event.which;
	else
		ntecla=event.keyCode;
	
	tecla = String.fromCharCode(ntecla);

	var m = mascara.charAt(elemento.value.length);

	var teclas = new Array(0, 8, 9, 13, 16, 17, 18, 20, 27, 28, 29, 30, 31);

	for (var i = 0; i < teclas.length; i ++) {
	  if (teclas[i] == ntecla) {
	    return true;
	  }
	}
	
	if ((m == '?') || (m == '#') || (m == '$')) return verificarMascara(tecla,m);
	else {
		elemento.value += mascara.charAt(elemento.value.length);
		m = mascara.charAt(elemento.value.length);
		if (mascara.charAt(elemento.value.length-1) == tecla) return false;
		else return verificarMascara(tecla,m);
	}
}

//------------------------------------------------------------------------------------------------------------
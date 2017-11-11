$(document).ready(function() {  
	$("#portNumber").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorPort").innerHTML = "Введите текст.";
			document.getElementById("errorPort").style.display = "block";
		}
		else if (variable < 0){
			$(this).addClass('invalid');
			document.getElementById("errorPort").innerHTML = "Номер порта - неотрицательное число.";
			document.getElementById("errorPort").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorPort").style.display = "none";
		}
	});
	
	$("#command").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorCommand").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorCommand").style.display = "none";
		}
	});
	
	$("#adressADC").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorAdressADC").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorAdressADC").style.display = "none";
		}
	});

	$("#adressDAC").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorAdressDAC").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorAdressDAC").style.display = "none";
		}
	});
	
	$("#timeGener").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorTimeGener").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorTimeGener").style.display = "none";
		}
	});

	$("#frequency").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorFrequency").innerHTML = "Введите значение частоты.";
			document.getElementById("errorFrequency").style.display = "block";
		}
		else if (variable < 0){
			$(this).addClass('invalid');
			document.getElementById("errorFrequency").innerHTML = "Значение частоты - неотрицательное число.";
			document.getElementById("errorFrequency").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorFrequency").style.display = "none";
		}
	});
	
	$("#amplitude").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorAmplitude").innerHTML = "Введите значение амплитуды.";
			document.getElementById("errorAmplitude").style.display = "block";
		}
		else if (variable < 0){
			$(this).addClass('invalid');
			document.getElementById("errorAmplitude").innerHTML = "Значение амплитуды - неотрицательное число.";
			document.getElementById("errorAmplitude").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorAmplitude").style.display = "none";
		}
	});
	
	$("#samplingFrequency").keyup(function(){
		var variable = $(this).val();
		if (variable.length == 0){
			$(this).addClass('invalid');
			document.getElementById("errorSamplingFrequency").innerHTML = "Введите значение частоты дискретизации.";
			document.getElementById("errorSamplingFrequency").style.display = "block";
		}
		else if (variable < 0){
			$(this).addClass('invalid');
			document.getElementById("errorSamplingFrequency").innerHTML = "Значение частоты дискретизации - неотрицательное число.";
			document.getElementById("errorSamplingFrequency").style.display = "block";
		}
		else if (variable > 1000){
			$(this).addClass('invalid');
			document.getElementById("errorSamplingFrequency").innerHTML = "Максимальное значение частоты дискретизации - 1000.";
			document.getElementById("errorSamplingFrequency").style.display = "block";
		}
		else if (variable < 2*document.getElementById("frequency").value){
			$(this).addClass('invalid');
			document.getElementById("errorSamplingFrequency").innerHTML = "Частота дискретизации должна быть более чем в 2 раза больше чатоты сигнала.";
			document.getElementById("errorSamplingFrequency").style.display = "block";
		}
		else{
			$(this).removeClass('invalid');
			document.getElementById("errorSamplingFrequency").style.display = "none";
		}
		
	});
	
	$('input').keyup();
});

function checkValidation(form){ 
	if(form == 'formPort'){ 
		$('#portNumber').keyup();
		return (document.getElementById("portNumber").getAttribute("class") != "invalid");
	} 
	
	if(form == 'formCommand'){
		$('#command').keyup();
		return (document.getElementById("command").getAttribute("class") != "invalid");
	}
	
	$('#frequency').keyup();
	$('#amplitude').keyup();
	$('#samplingFrequency').keyup();
	$("#adressADC").keyup();
	$("#adressDAC").keyup();
	$("#timeGener").keyup();
	return ((document.getElementById("frequency").getAttribute("class") != "invalid") && 
		(document.getElementById("amplitude").getAttribute("class") != "invalid") && 
		(document.getElementById("timeGener").getAttribute("class") != "invalid") &&
		(document.getElementById("adressADC").getAttribute("class") != "invalid") &&
		(document.getElementById("adressDAC").getAttribute("class") != "invalid"));
} 
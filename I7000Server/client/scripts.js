function AjaxFormRequest(form, button) { 
    if(!checkValidation(form)){ 
        alert('Некорректные данные'); 
        return; 
    } 
    jQuery.ajax({ 
        type: "GET", 
        dataType: "html", 
        data: jQuery("#"+form).serialize(),
        
        success: function(result) {
            if(form == 'formMeandr') {
                Meander(result);
            }
            else
            {
                $('#frame').attr('src', $('#frame').attr('src'));
                if(button.id == 'openPort') {
                    button.disabled = true;
                    document.getElementById('portNumber').disabled = true;
                    document.getElementById('speed').disabled = true;
                    document.getElementById('closePort').disabled = false;
                    document.getElementById('command').disabled = false;
                    document.getElementById('buttonCommand').disabled = false;
                    document.getElementById('frequency').disabled = false;
                    document.getElementById('amplitude').disabled = false;
                    document.getElementById('samplingFrequency').disabled = false;
                    document.getElementById('buttonMeandr').disabled = false;
                    document.getElementById('adressADC').disabled = false;
                    document.getElementById('adressDAC').disabled = false;
                    //document.getElementById('sendAdress').disabled = false;
                    document.getElementById('handed').disabled = false;
                }
                else if(button.id == 'closePort') {
                    button.disabled = true;
                    document.getElementById('portNumber').disabled = false;
                    document.getElementById('speed').disabled = false;
                    document.getElementById('openPort').disabled = false;
                    document.getElementById('command').disabled = true;
                    document.getElementById('buttonCommand').disabled = true;
                    document.getElementById('frequency').disabled = true;
                    document.getElementById('amplitude').disabled = true;
                    document.getElementById('samplingFrequency').disabled = true;
                    document.getElementById('buttonMeandr').disabled = true;
                    document.getElementById('adressADC').disabled = true;
                    document.getElementById('adressDAC').disabled = true;
                    //document.getElementById('sendAdress').disabled = true;
                    document.getElementById('handed').disabled = true;
                }
            }
        },
        statusCode:{ 
            425:function(e){
                if(button.id == 'openPort') {
                    button.disabled = true;
                    document.getElementById('portNumber').disabled = true;
                    document.getElementById('speed').disabled = true;
                    document.getElementById('closePort').disabled = false;
                    document.getElementById('command').disabled = false;
                    document.getElementById('buttonCommand').disabled = false;
                    document.getElementById('frequency').disabled = false;
                    document.getElementById('amplitude').disabled = false;
                    document.getElementById('samplingFrequency').disabled = false;
                    document.getElementById('buttonMeandr').disabled = false;
                    document.getElementById('adressADC').disabled = false;
                    document.getElementById('adressDAC').disabled = false;
                    //document.getElementById('sendAdress').disabled = false;
                    document.getElementById('handed').disabled = false;
                }
                else if(button.id == 'closePort') {
                    button.disabled = true;
                    document.getElementById('portNumber').disabled = false;
                    document.getElementById('speed').disabled = false;
                    document.getElementById('openPort').disabled = false;
                    document.getElementById('command').disabled = true;
                    document.getElementById('buttonCommand').disabled = true;
                    document.getElementById('frequency').disabled = true;
                    document.getElementById('amplitude').disabled = true;
                    document.getElementById('samplingFrequency').disabled = true;
                    document.getElementById('buttonMeandr').disabled = true;
                    document.getElementById('adressADC').disabled = true;
                    document.getElementById('adressDAC').disabled = true;
                    //document.getElementById('sendAdress').disabled = true;
                    document.getElementById('handed').disabled = true;
                }
                alert('Не удалось открыть порт'); 
            }, 
            427:function(){ 
                alert('Не удалось записать команду в порт'); 
            } 
        }
    })
    return false;
} 

google.charts.load('current', {'packages':['corechart']});
google.charts.setOnLoadCallback(drawChart);

function drawChart() {
    var data = google.visualization.arrayToDataTable([
                ['Время', 'Значение'],
                ['0',  0]
            ]);
            
    var options = {
        title: 'Ручной меандр',
        legend: { position: 'bottom' }
    };
    
    var chart = new google.visualization.LineChart(document.getElementById('chartHanded'));
    chart.draw(data, options);
        
    options = {
        title: 'Автоматический меандр',
        legend: { position: 'bottom' }
    };
    
    chart = new google.visualization.LineChart(document.getElementById('chartAuto'));
    chart.draw(data, options);
}

    function Meander(values){
        google.charts.load('current', {'packages':['corechart']});
        google.charts.setOnLoadCallback(drawChart);
        var arr = values.split(' '); 
        var n = arr.length + 1; 
        var vals = []; 
        
        vals[0] = []; 
        
        vals[0][0] = 'Время'; 
        vals[0][1] = 'Значение'; 
        
        var j = 1; 
        for(var i = 1; i < n; i+=2){ 
            vals[j] = []; 
            
            vals[j][0] = +arr[i-1]; 
            vals[j][1] = +arr[i]; 
            j++; 
        } 
            
        function drawChart() {
            var data = google.visualization.arrayToDataTable(vals);
            var options = (document.getElementById("handed").checked) ? 
                {title: 'Ручной меандр', legend: { position: 'bottom' }} : 
                {title: 'Автоматический меандр', legend: { position: 'bottom' }};

            var chart = (document.getElementById("handed").checked) ?
                new google.visualization.LineChart(document.getElementById('chartHanded')) :
                new google.visualization.LineChart(document.getElementById('chartAuto'));

            chart.draw(data, options);
        }
}
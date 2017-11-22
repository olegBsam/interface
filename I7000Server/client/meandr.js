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
            var options = {title: 'Меандр', legend: { position: 'bottom' }};

            var chart = new google.visualization.LineChart(document.getElementById('chart'));

            chart.draw(data, options);
        }
}
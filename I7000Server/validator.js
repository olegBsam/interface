			$(document).ready(function() {  
				$("#portNumber").keyup(function(){
					var variable = $(this).val();
					if (variable.length == 0 || variable < 0)
						$(this).addClass('invalid');
					else
						$(this).removeClass('invalid');
				});
				
				$("#command").keyup(function(){
					var variable = $(this).val();
					if (variable.length == 0)
						$(this).addClass('invalid');
					else
						$(this).removeClass('invalid');
				});
				
				$("#frequency").keyup(function(){
					var variable = $(this).val();
					if (variable.length == 0 || variable < 0)
						$(this).addClass('invalid');
					else
						$(this).removeClass('invalid');
				});
				
				$("#amplitude").keyup(function(){
					var variable = $(this).val();
					if (variable.length == 0 || variable < 0)
						$(this).addClass('invalid');
					else
						$(this).removeClass('invalid');
				});
				
				$("#samplingFrequency").keyup(function(){
					var variable = $(this).val();
					if (variable.length == 0 || variable < 0 || variable > 1000)
						$(this).addClass('invalid');
					else
						$(this).removeClass('invalid');
				});
				
				$('input').keyup();
			});
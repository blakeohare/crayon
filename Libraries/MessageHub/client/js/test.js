(() => {

	window.CrayonInterop.msghubRegisterConsumer(hub => {

		const { listen, send } = hub;

		listen('weather', (request, cb) => {
			let zip = request.zip;
			let forecast = "The weather for " + zip + " is BEEEEEEEES!!";
			cb(forecast);
		});

		setTimeout(() => {
			send('howAreYou', { askerName: 'Browser JavaScript' }, result => {
				console.log(result.msg);
			});
		}, 2000);

	});
})();

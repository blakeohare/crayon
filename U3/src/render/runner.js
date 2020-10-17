window.init = () => {
    registerMessageListener(data => {
        let hrh = document.getElementById('html_render_host');
        hrh.append(JSON.stringify(data));
        hrh.innerHTML += '<hr/>';
    });
};

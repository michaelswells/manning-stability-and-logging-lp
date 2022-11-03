let btnDefault = document.getElementById("check");
btnDefault.addEventListener('click', event => {
  var url = "http://localhost:5000/api/v1/health";
  fetch(
    url,
    {
      headers: {
        'X-DUMMY': 'dummy'
      }
    })
    .then(function(response) {
      if(response.ok || (response.status === 503)) {
        response
          .json()
          .then(function(data) {
            var health = document.getElementById('health');
            var healthHtml = '<p>Status: ' + data.status + '</p>';
            healthHtml += '<p>Message: ' + data.message + '</p>';
            health.innerHTML += healthHtml;
          });
      } else {
        response
          .text()
          .then(function(data) {
            console.log("response failed?");
            var health = document.getElementById('health');
            var healthHtml = '<p>Error: ' + response.status + '</p>';
            healthHtml += '<p>' + data + '</p>';
            health.innerHTML += healthHtml;
          });
      }
    })
    .catch(function(err) {
      console.log("error " + err);
    });
});

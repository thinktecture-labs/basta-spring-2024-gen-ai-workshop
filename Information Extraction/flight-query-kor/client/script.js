document.getElementById('queryForm').addEventListener('submit', function(event) {
    event.preventDefault();
    const query = document.getElementById('query').value;
    fetch('http://localhost:8000/filter', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ query: query })
    })
    .then(response => response.json())
    .then(data => {
        document.getElementById('departureAirport').value = data.departure_airport;
        document.getElementById('destinationAirport').value = data.destination_airport;
        document.getElementById('departureDate').value = data.departure_date;
        document.getElementById('returnDate').value = data.return_date;
        document.getElementById('airline').value = data.airline.join(', ');
        document.getElementById('class').value = data.class;
        document.getElementById('numberOfPersons').value = data.number_of_persons;
        document.getElementById('responseForm').style.display = 'block';
    })
    .catch(error => {
        console.error('Error:', error);
    });
});

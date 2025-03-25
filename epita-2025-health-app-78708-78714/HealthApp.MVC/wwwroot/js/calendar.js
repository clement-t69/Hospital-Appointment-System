document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('appointmentCalendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        events: JSON.parse(document.getElementById('calendarEventsData').textContent)
    });
    calendar.render();
});
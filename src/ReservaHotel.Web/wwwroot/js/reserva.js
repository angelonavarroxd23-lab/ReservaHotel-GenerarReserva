(function () {
    var form = document.getElementById('frmReserva');
    if (!form) return;

    // Pasos 5-7: al cambiar una fecha, el servidor recalcula la cantidad de días.
    ['FechaLlegada', 'FechaSalida'].forEach(function (id) {
        var el = document.getElementById(id);
        if (!el) return;
        el.addEventListener('change', function () {
            form.action = el.getAttribute('data-action');
            form.submit();
        });
    });

    // Enter en el código ejecuta "Buscar Cliente"; Enter en el huésped ejecuta "Agregar Habitación".
    function atajo(inputId, botonId) {
        var input = document.getElementById(inputId), boton = document.getElementById(botonId);
        if (!input || !boton) return;
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); if (!boton.disabled) boton.click(); }
        });
    }
    atajo('CodigoCliente', 'btn-buscar-cliente');
    atajo('NombreHuesped', 'btn-agregar-habitacion');

    // Seleccionar una fila de la cuadrícula marca su opción.
    document.querySelectorAll('#tabla-detalle tbody tr.fila-detalle').forEach(function (tr) {
        tr.addEventListener('click', function (e) {
            var r = tr.querySelector('input[type=radio]');
            if (r && e.target !== r) r.checked = true;
        });
    });
})();

// =================================================
// Expocentro · comportamiento de la interfaz
// =================================================

document.addEventListener("DOMContentLoaded", function () {

    // ---------- Menú lateral en móvil ----------
    const sidebar = document.getElementById("sidebar");
    const backdrop = document.getElementById("sidebarBackdrop");
    const toggle = document.getElementById("sidebarToggle");

    function cerrarMenu() {
        sidebar?.classList.remove("abierto");
        backdrop?.classList.remove("visible");
    }

    toggle?.addEventListener("click", function () {
        sidebar.classList.add("abierto");
        backdrop.classList.add("visible");
    });
    backdrop?.addEventListener("click", cerrarMenu);

    // ---------- Filas de tabla clicables ----------
    document.querySelectorAll("tr[data-href]").forEach(function (fila) {
        fila.classList.add("fila-link");
        fila.addEventListener("click", function (e) {
            if (e.target.closest("a, button, input, form")) return;
            window.location = fila.dataset.href;
        });
    });

    // ---------- Tarjetas seleccionables (montaje / servicios) ----------
    // Una tarjeta con data-ninguna="true" es excluyente ("Sin montaje").
    const grupos = document.querySelectorAll("[data-opciones]");

    grupos.forEach(function (grupo) {
        const tarjetas = grupo.querySelectorAll(".opcion-card");

        function refrescar() {
            tarjetas.forEach(function (t) {
                const check = t.querySelector(".form-check-input");
                t.classList.toggle("seleccionada", check.checked);
            });
        }

        tarjetas.forEach(function (tarjeta) {
            const check = tarjeta.querySelector(".form-check-input");
            const esNinguna = tarjeta.dataset.ninguna === "true";

            check.addEventListener("change", function () {
                if (check.checked) {
                    tarjetas.forEach(function (otra) {
                        if (otra === tarjeta) return;
                        const otraNinguna = otra.dataset.ninguna === "true";
                        if (esNinguna || otraNinguna) {
                            otra.querySelector(".form-check-input").checked = false;
                        }
                    });
                }
                refrescar();
            });
        });

        refrescar();
    });

    // ---------- Verificación de disponibilidad en vivo ----------
    const form = document.querySelector("form[data-disponibilidad]");
    if (form) {
        iniciarDisponibilidad(form);
    }
});

function iniciarDisponibilidad(form) {
    const url = form.dataset.disponibilidad;
    const excluirId = form.dataset.excluirId || "";
    const caja = document.getElementById("disponibilidad");

    const campo = function (nombre) {
        return form.querySelector("[name='" + nombre + "']");
    };

    const campos = ["SalonId", "FechaInicio", "HoraInicio", "FechaFin", "HoraFin", "CantidadPersonas"]
        .map(campo)
        .filter(Boolean);

    // Si cambia la fecha de inicio y la de fin queda antes, igualarla
    campo("FechaInicio")?.addEventListener("change", function () {
        const fin = campo("FechaFin");
        if (fin && (!fin.value || fin.value < this.value)) {
            fin.value = this.value;
        }
    });

    let temporizador;

    campos.forEach(function (c) {
        c.addEventListener("change", function () {
            clearTimeout(temporizador);
            temporizador = setTimeout(verificar, 250);
        });
    });

    function escapar(texto) {
        const div = document.createElement("div");
        div.textContent = texto ?? "";
        return div.innerHTML;
    }

    function mostrar(clase, html) {
        caja.className = "disp-box visible " + clase;
        caja.innerHTML = html;
    }

    async function verificar() {
        const salon = campo("SalonId")?.value;
        const fi = campo("FechaInicio")?.value;
        const hi = campo("HoraInicio")?.value;
        const ff = campo("FechaFin")?.value;
        const hf = campo("HoraFin")?.value;
        const personas = campo("CantidadPersonas")?.value;

        if (!salon || !fi || !hi || !ff || !hf) {
            caja.className = "disp-box";
            return;
        }

        const inicio = fi + "T" + hi;
        const fin = ff + "T" + hf;

        if (fin <= inicio) {
            mostrar("disp-error", '<i class="bi bi-x-circle me-1"></i> La finalización debe ser posterior al inicio.');
            return;
        }

        const params = new URLSearchParams({ salonId: salon, inicio: inicio, fin: fin, excluirId: excluirId, personas: personas || "" });

        try {
            const resp = await fetch(url + "?" + params.toString(), { headers: { "Accept": "application/json" } });
            if (!resp.ok) return;
            const d = await resp.json();
            if (!d.valido) return;

            const avisos = [];
            if (d.excedeCapacidad) {
                avisos.push("La cantidad de personas excede la capacidad del salón (" + d.capacidad + ").");
            }

            const lista = d.conflictos.length
                ? "<ul>" + d.conflictos.map(function (c) {
                    return "<li><strong>" + escapar(c.nombreEvento) + "</strong> · " + escapar(c.inicio) + " – " + escapar(c.fin) + " · " + escapar(c.estado) + "</li>";
                }).join("") + "</ul>"
                : "";

            if (d.bloqueado) {
                mostrar("disp-error", '<i class="bi bi-x-octagon me-1"></i> <strong>Salón no disponible</strong> en ese horario.' + lista +
                    (avisos.length ? "<div class='mt-1'>" + avisos.join(" ") + "</div>" : ""));
            } else if (d.conflictos.length || avisos.length) {
                mostrar(avisos.length ? "disp-error" : "disp-warn",
                    '<i class="bi bi-exclamation-triangle me-1"></i> ' +
                    (avisos.length ? avisos.join(" ") : "<strong>Disponible con advertencia:</strong> hay solicitudes en trámite para ese horario.") +
                    lista);
            } else {
                mostrar("disp-ok", '<i class="bi bi-check-circle me-1"></i> <strong>' + escapar(d.salon) + "</strong> está disponible en ese horario (capacidad " + d.capacidad + ").");
            }
        } catch (e) {
            caja.className = "disp-box";
        }
    }

    verificar();
}

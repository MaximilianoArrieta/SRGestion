window.sr = {
  leer: function (clave) { try { return localStorage.getItem(clave); } catch (e) { return null; } },
  guardar: function (clave, valor) { try { localStorage.setItem(clave, valor); } catch (e) { } },
  borrar: function (clave) { try { localStorage.removeItem(clave); } catch (e) { } },

  temaClaro: function () { return document.documentElement.getAttribute("data-theme") === "light"; },
  cambiarTema: function () {
    var nuevo = sr.temaClaro() ? "dark" : "light";
    document.documentElement.setAttribute("data-theme", nuevo);
    try { localStorage.setItem("sr-tema", nuevo); } catch (e) { }
    return nuevo === "light";
  },

  descargar: function (nombre, base64, tipo) {
    var bytes = Uint8Array.from(atob(base64), function (c) { return c.charCodeAt(0); });
    var url = URL.createObjectURL(new Blob([bytes], { type: tipo }));
    var a = document.createElement("a");
    a.href = url;
    a.download = nombre;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 2000);
  }
};

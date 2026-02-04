using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace BackendAtlas.Hubs
{
    [Authorize] // Asegurar que solo usuarios autenticados puedan conectarse
    public class PedidosHub : Hub
    {
        // Método para que el cliente se una al grupo de su sucursal
        public async Task JoinSucursalGroup(string sucursalId)
        {
            // Validar si el usuario tiene permiso para esa sucursal (opcional pero recomendado)
            // Por simplicidad, unimos al grupo. 
            // En un escenario real, deberíamos validar Claims o db.
            await Groups.AddToGroupAsync(Context.ConnectionId, sucursalId);
        }

        public async Task LeaveSucursalGroup(string sucursalId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sucursalId);
        }
    }
}

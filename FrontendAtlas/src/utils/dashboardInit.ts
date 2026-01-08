import { $userStore, logout } from '../store/auth';

export function initDashboard() {
  const user = $userStore.get();
  
  if (!user || !user.token) {
    window.location.href = '/login';
    return;
  }

  const mainContent = document.getElementById('mainContent');
  if (!mainContent) return;

  if (user.role === 'SuperAdmin') {
    mainContent.innerHTML = `
      <div class="p-6">
        <div class="flex justify-between items-center mb-6">
          <h1 class="text-3xl font-bold">Negocios Registrados</h1>
          <button class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">Registrar Nuevo Restaurante</button>
        </div>
        <div class="overflow-x-auto">
          <table class="min-w-full bg-white border border-gray-200">
            <thead>
              <tr>
                <th class="px-4 py-2 border-b">Nombre</th>
                <th class="px-4 py-2 border-b">Propietario</th>
                <th class="px-4 py-2 border-b">Sucursales</th>
                <th class="px-4 py-2 border-b">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td class="px-4 py-2 border-b">Restaurante Ejemplo</td>
                <td class="px-4 py-2 border-b">Juan Pérez</td>
                <td class="px-4 py-2 border-b">3</td>
                <td class="px-4 py-2 border-b">
                  <button class="bg-green-500 text-white px-2 py-1 rounded mr-2 hover:bg-green-600">Ver Sucursales</button>
                  <button class="bg-red-500 text-white px-2 py-1 rounded hover:bg-red-600">Dar de Baja</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    `;
  } else if (user.role === 'AdminNegocio') {
    mainContent.innerHTML = `
      <div class="p-6">
        <h1 class="text-3xl font-bold mb-6">Mis Sucursales</h1>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div class="bg-white p-6 rounded-lg shadow-md">
            <h2 class="text-xl font-semibold mb-4">Sucursal Centro</h2>
            <p class="text-gray-600 mb-4">Dirección: Calle Principal 123</p>
            <div class="flex space-x-2">
              <button class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">Gestionar Carta</button>
              <button class="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">Ver Pedidos</button>
            </div>
          </div>
          <div class="bg-white p-6 rounded-lg shadow-md">
            <h2 class="text-xl font-semibold mb-4">Sucursal Norte</h2>
            <p class="text-gray-600 mb-4">Dirección: Avenida Norte 456</p>
            <div class="flex space-x-2">
              <button class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">Gestionar Carta</button>
              <button class="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">Ver Pedidos</button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Botón de logout
  const logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
      logout();
      window.location.href = '/login';
    });
  }
}
import type { UserData } from "../stores/auth";

export function getUserFromStorage(): UserData | null {
    if (typeof window === 'undefined') return null;

    try {
        const rawData = sessionStorage.getItem("userStore") || localStorage.getItem("userStore");
        if (!rawData) return null;

        const parsed = JSON.parse(rawData);
        // Maneja la estructura de Nano Stores { state: { user: ... } } o directa
        const user = parsed.state?.user || parsed;

        // Validación mínima
        if (user && typeof user === 'object') {
            return user as UserData;
        }
        return null;
    } catch (e) {
        console.error("Error parsing user data:", e);
        return null;
    }
}

export function getUserRole(): string {
    const user = getUserFromStorage();
    if (!user) return '';
    return (user.role || (user as any).rol || (user as any).Role || '').toString();
}

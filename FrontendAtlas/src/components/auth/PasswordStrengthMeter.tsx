import React, { useMemo } from 'react';

interface Props {
    password: string;
}

export const PasswordStrengthMeter: React.FC<Props> = ({ password }) => {
    const strength = useMemo(() => {
        let score = 0;
        if (!password) return 0;

        if (password.length > 8) score += 1;
        if (password.length > 12) score += 1;
        if (/[A-Z]/.test(password)) score += 1;
        if (/[a-z]/.test(password)) score += 1;
        if (/[0-9]/.test(password)) score += 1;
        if (/[^A-Za-z0-9]/.test(password)) score += 1;

        return Math.min(score, 5); // Max 5 (0-5 scale really effectively 0-4 + length bonuses)
    }, [password]);

    const getColor = (score: number) => {
        if (score <= 1) return 'bg-red-500';
        if (score <= 2) return 'bg-orange-500';
        if (score <= 3) return 'bg-yellow-500';
        if (score >= 4) return 'bg-green-500';
        return 'bg-gray-200';
    };

    const getLabel = (score: number) => {
        if (score <= 1) return 'Débil';
        if (score <= 2) return 'Regular';
        if (score <= 3) return 'Buena';
        if (score >= 4) return 'Fuerte';
        return '';
    };

    if (!password) return null;

    return (
        <div className="mt-2">
            <div className="flex justify-between items-center mb-1">
                <span className="text-xs text-gray-500 font-medium">Fortaleza</span>
                <span className="text-xs text-gray-700 font-semibold">{getLabel(strength)}</span>
            </div>
            <div className="h-1.5 w-full bg-gray-200 rounded-full overflow-hidden">
                <div
                    className={`h-full transition-all duration-300 ${getColor(strength)}`}
                    style={{ width: `${(strength / 5) * 100}%` }}
                />
            </div>
            <ul className="mt-2 space-y-1">
                <Requirementmet met={password.length >= 8} text="Mínimo 8 caracteres" />
                <Requirementmet met={/[A-Z]/.test(password)} text="Mayúscula" />
                <Requirementmet met={/[a-z]/.test(password)} text="Minúscula" />
                <Requirementmet met={/[0-9]/.test(password)} text="Número" />
            </ul>
        </div>
    );
};

const Requirementmet = ({ met, text }: { met: boolean; text: string }) => (
    <li className={`text-xs flex items-center ${met ? 'text-green-600' : 'text-gray-400'}`}>
        <span className={`mr-1.5 inline-block w-1.5 h-1.5 rounded-full ${met ? 'bg-green-500' : 'bg-gray-300'}`}></span>
        {text}
    </li>
);

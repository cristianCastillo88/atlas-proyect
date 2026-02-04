import { useRef, useCallback, useState } from 'react';
import { useReactToPrint } from 'react-to-print';
import type { Pedido, Sucursal } from '../types/db';

export function usePrintTicket() {
    // References to the component to print
    const printRef = useRef<HTMLDivElement>(null);

    // State to hold the data currently being printed
    const [printingOrder, setPrintingOrder] = useState<Pedido | null>(null);
    const [printingSucursal, setPrintingSucursal] = useState<Sucursal | null>(null);

    // The actual print function from the library
    const reactToPrintFn = useReactToPrint({
        contentRef: printRef,
        documentTitle: `Ticket-${printingOrder?.id || 'new'}`,
        pageStyle: `
            @page {
                size: auto;
                margin: 0mm;
            }
            @media print {
                body {
                    -webkit-print-color-adjust: exact;
                }
            }
        `,
        onAfterPrint: () => {
            // Optional: Reset state or show success toast
            // console.log('Printed successfully');
        },
    });

    // Helper to setup data and trigger print
    const handlePrint = useCallback((order: Pedido, sucursal: Sucursal) => {
        setPrintingOrder(order);
        setPrintingSucursal(sucursal);

        // Timeout to allow state to update and DOM to render the new props before printing
        // React batching might delay the render, so we wait slightly.
        setTimeout(() => {
            reactToPrintFn();
        }, 100);
    }, [reactToPrintFn]);

    return {
        printRef,
        handlePrint,
        printingOrder,
        printingSucursal
    };
}

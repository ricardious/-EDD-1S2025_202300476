using System;
using AutoGestPro.src.Models;

namespace AutoGestPro.DataStructures
{
    public unsafe class InvoiceStack
    {
        private Invoice* top;
        
        public InvoiceStack()
        {
            top = null;
        }
        
        public void Push(Invoice* newInvoice)
        {
            newInvoice->Next = top;
            top = newInvoice;
        }
        
        public Invoice* Pop()
        {
            if (top == null)
                return null;
                
            Invoice* invoice = top;
            top = top->Next;
            invoice->Next = null;
            return invoice;
        }
        
        public Invoice* Peek()
        {
            return top;
        }
        
        public string GenerateDot()
        {
            string dot = "digraph InvoiceStack {\n";
            dot += "rankdir=TB;\n";
            dot += "node [shape=record];\n";
            dot += "label=\"Invoice Stack\";\n";
            
            Invoice* current = top;
            int i = 0;
            
            while (current != null)
            {
                dot += $"node{i} [label=\"ID: {current->ID}| Order: {current->ID_Orden}| Total: {current->Total:C}\"];\n";
                
                if (current->Next != null)
                {
                    dot += $"node{i} -> node{i+1};\n";
                }
                
                current = current->Next;
                i++;
            }
            
            dot += "}";
            return dot;
        }
    }
}

using System;
using AutoGestPro.src.Models;

namespace AutoGestPro.DataStructures
{
    public unsafe class ServiceQueue
    {
        private Service* front;
        private Service* rear;
        
        public ServiceQueue()
        {
            front = null;
            rear = null;
        }
        
        public void Enqueue(Service* newService)
        {
            if (front == null)
            {
                front = newService;
                rear = newService;
                return;
            }
            
            rear->Next = newService;
            rear = newService;
        }
        
        public Service* Dequeue()
        {
            if (front == null)
                return null;
                
            Service* service = front;
            front = front->Next;
            
            if (front == null)
                rear = null;
                
            service->Next = null;
            return service;
        }
        
        public Service* Search(int id)
        {
            Service* current = front;
            while (current != null)
            {
                if (current->ID == id)
                {
                    return current;
                }
                current = current->Next;
            }
            return null;
        }
        
        public string GenerateDot()
        {
            string dot = "digraph ServiceQueue {\n";
            dot += "rankdir=LR;\n";
            dot += "node [shape=record];\n";
            dot += "label=\"Service Queue\";\n";
            
            Service* current = front;
            int i = 0;
            
            while (current != null)
            {
                dot += $"node{i} [label=\"ID: {current->ID}| Vehicle: {current->Id_Vehiculo}| Part: {current->Id_Repuesto}| Cost: {current->Costo:C}\"];\n";
                
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

import React, { useState } from 'react'
import { EmployeeTable } from './EmployeeTable'
import { AddEmployeeForm } from './AddEmployeeForm'
import { EditEmployeeForm } from './EditEmployeeForm'

const EmployeeCRUDApp = () => {

    const employeesData = [
        { id: 1, name: 'Tania', username: 'floppydiskette' },
        { id: 2, name: 'Craig', username: 'siliconeidolon' },
        { id: 3, name: 'Mark', username: 'markhazleton' },
    ]

    const [employees, setEmployees] = useState(employeesData)
    const initialFormState = { id: null, name: '', username: '' }
    const [editing, setEditing] = useState(false)
    const [currentEmployee, setCurrentEmployee] = useState(initialFormState)

    const addEmployee = employee => {
        employee.id = employees.length + 1
        setEmployees([...employees, employee])
    }

    const deleteEmployee = id => {
        setEmployees(employees.filter(employee => employee.id !== id))
    }

    const editRow = employee => {
        setEditing(true)
        setCurrentEmployee({ id: employee.id, name: employee.name, username: employee.username })
    }
    const updateEmployee = (id, updateEmployee) => {
        setEditing(false)
        setEmployees(employees.map(employee => (employee.id === id ? updateEmployee : employee)))
    }

    return (
        <div className="container">
            <h1>Employee CRUD</h1>
            <div className="row">
                <div className="col">
                    {editing ? (
                        <div>
                            <EditEmployeeForm
                                setEditing={setEditing}
                                currentEmployee={currentEmployee}
                                updateEmployee={updateEmployee}
                            />
                        </div>
                    ) : (
                            <div>
                                <AddEmployeeForm addEmployee={addEmployee} />
                            </div>
                        )}
                </div>
                <div className="col">
                    <h2>View employees</h2>
                    <EmployeeTable employees={employees} deleteEmployee={deleteEmployee} editRow={editRow} />
                </div>
            </div>
        </div>
    )
}

export default EmployeeCRUDApp

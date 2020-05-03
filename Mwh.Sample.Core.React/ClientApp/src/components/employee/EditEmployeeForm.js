import React, { useState } from 'react';

export const EditEmployeeForm = props => {
    const [employee, setEmployee] = useState(props.currentEmployee)

    const handleInputChange = event => {
        const { name, value } = event.target
        setEmployee({ ...employee, [name]: value })
    }

    return (
        <>
            <h1>Edit Employee</h1>
            <form
                onSubmit={event => {
                    event.preventDefault()
                    if (!employee.name || !employee.username) return
                    props.updateEmployee(employee)
                }}
            >
                <div className="form-group">
                    <label htmlFor="name">Name</label>
                    <input
                        type="text"
                        name="name"
                        className="form-control"
                        value={employee.name}
                        onChange={handleInputChange}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="username">Username</label>
                    <input
                        type="text"
                        name="username"
                        className="form-control"
                        value={employee.username}
                        onChange={handleInputChange}
                    />
                </div>
                <div className="form-group">
                    <button name="submit" type="submit" className="btn btn-primary">Update Employee</button>
                    <button name="cancel" onClick={()=> props.setEditing(false)} className="btn btn-warning">Cancel</button>
                </div>
            </form>
        </>
    )
}
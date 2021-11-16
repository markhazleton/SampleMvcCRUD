import React from 'react';

export const EmployeeTable = props => (
    <table>
        <thead>
            <tr>
                <th>Name</th>
                <th>Username</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            {props.employees.length > 0 ? (
                props.employees.map(employee => (
                    <tr key={employee.id}>
                        <td>{employee.name}</td>
                        <td>{employee.username}</td>
                        <td>
                            <button onClick={() => { props.editRow(employee) }} className="btn btn-primary">Edit</button> |
                            <button onClick={() => props.deleteEmployee(employee.id)} className="btn btn-primary">Delete</button>
                        </td>
                    </tr>))) : (<tr><td colSpan={3}>no employees</td></tr>)
            }

        </tbody>
    </table >
);
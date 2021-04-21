import React, { useState } from "react";

export const AddEmployeeForm = (props) => {
  const initialFormState = { id: null, name: "", username: "" };
  const [employee, setEmployee] = useState(initialFormState);
  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setEmployee({ ...employee, [name]: value });
  };

  return (
    <>
      <h1>Add Employee</h1>
      <form
        onSubmit={(event) => {
          event.preventDefault();
          if (!employee.name || !employee.username) return;
          props.addEmployee(employee);
          setEmployee(initialFormState);
        }}
      >
        <div className="row">
          <div className="form-group col">
            <label htmlFor="name">Name</label>
            <input
              type="text"
              name="name"
              className="form-control"
              value={employee.name}
              onChange={handleInputChange}
            />
          </div>
          <div className="form-group col">
            <label htmlFor="username">Username</label>
            <input
              type="text"
              name="username"
              className="form-control"
              value={employee.username}
              onChange={handleInputChange}
            />
          </div>
          <div className="form-group col">
            <button name="submit" type="submit" className="btn btn-primary">
              Add
            </button>
          </div>
        </div>
      </form>
    </>
  );
};

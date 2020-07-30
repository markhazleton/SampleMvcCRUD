import React, { Component } from 'react';
import { Nav, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
    
    static displayName = NavMenu.name;
    
    constructor(props) {
        super(props);
        
        this.toggleNavbar = this.toggleNavbar.bind(this);
        this.state = {
            collapsed: true
        };
    }
    
    toggleNavbar() {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }
    
    render() {
        
        return (
            <header>

                <div>
      <Nav tabs>
        <NavItem>
          <NavLink href="/#" active>Home</NavLink>
        </NavItem>
        <NavItem>
            <NavLink tag={Link} className="text-dark" to="/author-quiz">Author Quiz</NavLink>
                                        </NavItem>
                                        <NavItem>
                                            <NavLink tag={Link} className="text-dark" to="/employee-crud">Employee CRUD</NavLink>
                                        </NavItem>
                                        <NavItem>
                                            <NavLink tag={Link} className="text-dark" to="/github">GitHub Card</NavLink>
                                        </NavItem>
                                        <NavItem>
                                            <NavLink tag={Link} className="text-dark" to="/counter">Counter</NavLink>
                                        </NavItem>
                                        <NavItem>
                                            <NavLink tag={Link} className="text-dark" to="/fetch-data">Fetch data</NavLink>
                                        </NavItem>
      </Nav>
    </div>



            </header>
        );
    }
}

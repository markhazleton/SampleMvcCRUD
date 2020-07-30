import React, { Component } from 'react';
import { Container, Navbar, NavbarBrand } from 'reactstrap';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div>
        <NavMenu />
        <Container key="MainContainer">
          {this.props.children}
        </Container>
        <div className="fixed-bottom">  
            <Navbar color="dark" dark>
                <Container>
                    <NavbarBrand>Mwh Sample Core React</NavbarBrand>
                    <a target="_blank" href="https://linkedin.com/in/markhazleton">Mark Hazleton</a>
                </Container>
            </Navbar>
        </div>
      </div>
    );
  }
}

import React, { Component, useState } from 'react';

export class Counter extends Component {
    static displayName = Counter.name;

    constructor(props) {
        super(props);
        this.state = { currentCount: 0, buttonPushed: 0 };
        this.incrementCounter = this.incrementCounter.bind(this);
        this.pushButton = this.pushButton.bind(this);
    }

    incrementCounter() {
        this.setState({
            currentCount: this.state.currentCount + 1,
        });
    }

    pushButton(targetValue) {
        this.setState({
            buttonPushed: targetValue,
        });
    }


    render() {
        return (
            <div className="container">
                <div className="row" style={{ marginTop: "5px", marginBottom: "5px" }}>
                    <div className="col-6">
                        <div className="card" >
                            <h2 className="card-header">Counter</h2>
                            <div className="card-body">
                                <p>This is an <strong>example</strong> of a React component.</p>
                                <p aria-live="polite">Count: <strong>{this.state.currentCount}</strong></p>
                                <button className="btn btn-primary" onClick={this.incrementCounter} >Increment</button>
                            </div>
                        </div>
                    </div>
                    <div className="col-6">
                        <div className="card" >
                            <h2 className="card-header">Multiple Buttons</h2>
                            <div className="card-body">
                                <MyApp />
                            </div>
                        </div>
                    </div>
                </div >
                <div className="row" style={{ marginTop: "5px", marginBottom: "5px" }}>
                    <div className="col-6">
                        <div className="card" >
                            <h2 className="card-header">Click on HTML Element</h2>
                            <div className="card-body">
                                <MyElement />
                            </div>
                        </div>
                    </div>
                    <div className="col-6">
                        <div className="card" >
                            <h2 className="card-header">Generate Buttons</h2>
                            <div className="card-body">
                                <p aria-live="polite">Count: <strong>{this.state.buttonPushed}</strong></p>
                                <ClickyButtons numberOfButtons={99} onSelection={this.pushButton} />
                            </div>
                        </div>
                    </div>
                </div>
            </div >
        );
    }
}

function MyApp() {
    const [counter, setCounter] = useState(0);
    const incrementCounter = (incrementValue) => setCounter(counter + incrementValue);
    return (
        <div>
            <MyButton onClickFunction={incrementCounter} increment={1} />
            <MyButton onClickFunction={incrementCounter} increment={5} />
            <MyButton onClickFunction={incrementCounter} increment={10} />
            <MyButton onClickFunction={incrementCounter} increment={100} />
            <MyButton onClickFunction={incrementCounter} increment={1000} />
            <MyDisplay message={counter} />
        </div>
    );
}

function MyDisplay(props) {
    return (
        <div style={{ fontWeight: "bolder" }}>Count: {props.message}</div>
    );
}

function MyButton(props) {
    const handleClick = () => props.onClickFunction(props.increment);
    return <button className="btn btn-primary" onClick={handleClick} style={{ marginLeft: "2px", marginRight: "2px" }}>
        +{props.increment}
    </button>;
}


const myProps = { a: 4, b: 5 };
function MyElement() {
    return <SumClass {...myProps} />;
}
function range(size, startAt = 1) {
    return [...Array(size).keys()].map(i => i + startAt);
}
function ClickyButtons({ numberOfButtons, onSelection }) {
    const makeButton = v =>
        <button
            key={v}
            id={v}
            onClick={event => onSelection(event.target.id)}
        >
            {v}
        </button>;
    return <div>
        {range(numberOfButtons).map(makeButton)}
    </div>;
}

class SumClass extends React.Component {
    constructor(props) {
        super(props);
        this.state = { clicks: 0, user: 'Unknown' };
    }

    render() {
        return (
            <div>
                <p>{this.props.a} + {this.props.b} = {this.props.a + this.props.b} </p>
                <p onClick={() => { this.setState({ clicks: this.state.clicks + 1, user: 'Mark' }); }}>
                    Clicked {this.state.clicks} times. - says {this.state.user}
                </p>
            </div>
        );
    }
}

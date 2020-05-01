import React, { Component } from 'react';
import axios from 'axios';
import './GitHubApp.css'

const CardList = (props) => (
    <div>
        {props.profiles.map(profile => <Card {...profile} key={profile.name} />)}
    </div>
);

class Card extends Component {
    render() {
        const profile = this.props;
        return (
            <div className="github-profile">
                <a href={profile.html_url} >
                    <img src={profile.avatar_url} alt={profile.name} />
                </a>
                <div className="info">
                    <div className="name">{profile.name}</div>
                    <div className="company">{profile.bio}</div>
                </div>
            </div>
        );
    }
}

class Header extends Component {
    render() {
        return (
            <div className="header">{this.props.title}</div>
        );
    }
}

class GitHubForm extends Component {
    state = { userName: '' };

    handleSubmit = async (event) => {
        event.preventDefault();
        const resp = await axios.get(`https://api.github.com/users/${this.state.userName}`);
        this.props.onSubmit(resp.data);
        this.setState({ userName: '' });
    };


    render() {
        return (
            <form onSubmit={this.handleSubmit}>
                <input
                    type="text"
                    value={this.state.userName}
                    onChange={event => this.setState({ userName: event.target.value })}
                    placeholder="GitHub username"
                    requried="true"
                />

                <button>Add card</button>
            </form>
        );
    }
}

export class GitHubApp extends Component {

    state = {
        profiles: [],
    };
    addNewProfile = async (profileData) => {

        const respFollowing = await axios.get(`https://api.github.com/users/${profileData.login}/following`);
        console.log(respFollowing.data);

        this.setState(prevState => ({
            profiles: [...prevState.profiles, profileData],
        }));


    }
    render() {
        return (
            <>
                <Header title="The GitHub Cards App" />
                <GitHubForm onSubmit={this.addNewProfile} />
                <CardList profiles={this.state.profiles} />
            </>
        );
    }
}




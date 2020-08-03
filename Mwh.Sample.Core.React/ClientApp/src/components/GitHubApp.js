import React, { Component } from 'react';
import axios from 'axios';
import './GitHubApp.css'

const CardList = (props) => (
    <div className='card-deck'>
        {props.profiles.map(profile => <Card {...profile} key={profile.name} />)}
    </div>
);

class Card extends Component {
    render() {
        const profile = this.props;
        return (
            <div className="card col-4 github-profile">
                   <img src={profile.avatar_url} className="card-img-top img-thumbnail" alt="{profile.name}" />
                <div className="card-body">
                    <h5 className="card-title">
                        {profile.name}
                    </h5>
                    <p className="card-text">{profile.bio}</p>
                    <ul>{profile.followingList.map((li, i) => <li key={i}>{li}</li>)}</ul> 
                     <a href={profile.html_url} className="btn btn-primary">{profile.name}</a>
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

    constructor(props) {
        super(props);
        this.loadProfile('markhazleton');
    }

    loadProfile = async (profile) => {
        const resp = await axios.get(`https://api.github.com/users/${profile}`);
        resp.data.followingList = [];
        const respFollowing = await axios.get(`https://api.github.com/users/${resp.data.login}/following`);
        for (var i = 0; i < respFollowing.data.length; i++) {
            resp.data.followingList.push(respFollowing.data[i].login);
        }
        this.props.onSubmit(resp.data);
    }

    handleSubmit = async (event) => {
        event.preventDefault();
        this.loadProfile(this.state.userName);
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
                <button>Add Profile</button>
            </form>
        );
    }
}

export class GitHubApp extends Component {

    state = {
        profiles: [],
    };

    addNewProfile = async (profileData) => {
        this.setState(prevState => ({
            profiles: [...prevState.profiles, profileData],
        }));
    }

    render() {
        return (
            <>
                <Header title="GitHub Profiles" />
                <GitHubForm onSubmit={this.addNewProfile} />
                <CardList profiles={this.state.profiles} />
            </>
        );
    }
}




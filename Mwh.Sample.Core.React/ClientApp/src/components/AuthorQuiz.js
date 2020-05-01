import React from 'react';
import PropTypes from 'prop-types';
import ReactDOM from 'react-dom';
import { shuffle, sample } from 'underscore';
import './App.css';

const authors = [
    {
        name: 'Mark Twain',
        imageUrl: 'images/authors/marktwain.jpg',
        imageSource: 'Wikimedia Commons',
        books: ['The Adventures of Huckleberry Finn']
    },
    {
        name: 'Joseph Conrad',
        imageUrl: 'images/authors/josephconrad.png',
        imageSource: 'Wikimedia Commons',
        books: ['Heart of Darkness']
    },
    {
        name: 'J.K. Rowling',
        imageUrl: 'images/authors/jkrowling.jpg',
        imageSource: 'Wikimedia Commons',
        imageAttribution: 'Daniel Ogren',
        books: ['Harry Potter and the Sorcerers Stone']
    },
    {
        name: 'Stephen King',
        imageUrl: 'images/authors/stephenking.jpg',
        imageSource: 'Wikimedia Commons',
        imageAttribution: 'Pinguino',
        books: ['The Shining', 'IT']
    },
    {
        name: 'Charles Dickens',
        imageUrl: 'images/authors/charlesdickens.jpg',
        imageSource: 'Wikimedia Commons',
        books: ['David Copperfield', 'A Tale of Two Cities']
    },
    {
        name: 'William Shakespeare',
        imageUrl: 'images/authors/williamshakespeare.jpg',
        imageSource: 'Wikimedia Commons',
        books: ['Hamlet', 'Macbeth', 'Romeo and Juliet']
    }
];

function getTurnData(authors) {
    const allBooks = authors.reduce(function (p, c, i) {
        return p.concat(c.books);
    }, []);
    const fourRandomBooks = shuffle(allBooks).slice(0, 4);
    const answer = sample(fourRandomBooks);

    return {
        books: fourRandomBooks,
        author: authors.find((author) =>
            author.books.some((title) =>
                title === answer))
    }
}

function resetState() {
    return {
        turnData: getTurnData(authors),
        highlight: ''
    };
}

let state = resetState();

function onAnswerSelected(answer) {
    const isCorrect = state.turnData.author.books.some((book) => book === answer);
    state.highlight = isCorrect ? 'correct' : 'wrong';
    AuthorQuizRender();
}
function Hero() {
    return (<div className="row">
        <div className="jumbotron col-10 offset-1">
            <h1>Author Quiz</h1>
            <p>Select the book written by the author shown</p>
        </div>
    </div>
    );
}

function Book({ title, onClick }) {
    return (<div className="answer" onClick={() => { onClick(title); }} >
        <h4>{title}</h4>
    </div>
    );
}

function Turn({ author, books, highlight, onAnswerSelected }) {

    function hightlightToBgColor(highlight) {
        const mapping = {
            'none': '',
            'correct': 'green',
            'wrong': 'red'
        };
        return mapping[highlight];
    }
    return (<div key="turnMain" className="row turn" style={{ backgroundColor: hightlightToBgColor(highlight) }}>
        <div key="authorImageContainer" className="col-r offset-1">
            <img key="authorImage" src={author.imageUrl} className="authorimage" alt="Author" />
        </div>
        <div key="bookList" className="col-6">
            {books.map((title) => <Book key={title} title={title} onClick={onAnswerSelected} />)}
        </div>
    </div>
    );
}

Turn.propTypes = {
    author: PropTypes.shape({
        name: PropTypes.string.isRequired,
        imageUrl: PropTypes.string.isRequired,
        imageSource: PropTypes.string.isRequired,
        books: PropTypes.arrayOf(PropTypes.string).isRequired
    }),
    books: PropTypes.arrayOf(PropTypes.string).isRequired,
    onAnswerSelected: PropTypes.func.isRequired,
    highlight: PropTypes.string.isRequired
}

function Continue({ show, onContinue }) {
    return (
        <div className="row continue">
            {show
                ? <div className="col-11">
                    <button className="btn btn-primary btn-lg float-right" onClick={onContinue}>Continue</button>
                </div>
                : null}
        </div>
    );
}

function Footer() {
    return (<footer id="footer" className="row">
        <div className="col-12">
            <p className="text-muted credit">
                All images are from <a href="https://commons.wikimedia.org/wiki/Main_Page">Wikimedia Commons</a> and are in the public domain.
      </p>
        </div>
    </footer>
    );
}


function AuthorQuiz({ turnData, highlight, onAnswerSelected, onContinue }) {
    return (
        <>
            <div className="container-fluid" id="AuthorQuizContainer">
                <Hero />
                <Turn {...turnData} highlight={highlight} onAnswerSelected={onAnswerSelected} />
                <Continue show={highlight === 'correct'} onContinue={onContinue} />
                <Footer />
            </div>
        </>
    );
}

function AuthorQuizRender() {
    ReactDOM.render(
        <AuthorQuizApp />, document.getElementById('AuthorQuizContainer')
    );
}
function AuthorQuizApp() {
    return <AuthorQuiz {...state}
        onAnswerSelected={onAnswerSelected}
        onContinue={() => {
            state = resetState();
            AuthorQuizRender();
        }} />;
}

export default AuthorQuizApp;

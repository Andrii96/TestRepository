import _ from 'lodash';

const shared = {
    overlay: {
        backgroundColor: 'rgba(0, 0, 0, 0.5)'
    },
    content: {
        position: 'relative',
        background: '#fff',
        borderRadius: '0',
        border: 'none',
        padding: '0'
    }
};

const desktop = _.defaultsDeep({
    overlay: {
        overflowY: 'auto',
        overflowX: 'hidden',
        width: '100%',
        minHeight: '100%'
    },
    content: {
        top: '60px',
        left: 'auto',
        right: 'auto',
        bottom: 'auto',
        margin: '30px auto',
        borderRadius: '5px',
        width: '90%',
        overflow: 'visible'
    },
}, shared);

const mobile = _.defaultsDeep({
    overlay: {
        overflowX: 'hidden',
        overflowY: 'auto',
    },
    content: {
        top: '10px',
        left: '10px',
        right: 'auto',
        bottom: 'auto',
        width: 'calc(100% - 20px)',
    },
}, shared);

export default {
    desktop,
    mobile,
};

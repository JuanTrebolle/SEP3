package dk.groupfive.ModeratorServer.remote;

import dk.groupfive.ModeratorServer.model.objects.User;

public interface Server {
    boolean authorizeUser(User user) throws Exception;

    //using just id to save some bandwidth
    void removePlace(long placeId);

    void removeReview(long reviewId);

    void banUser(long userId);

    void unbanUser(long userId);
}
